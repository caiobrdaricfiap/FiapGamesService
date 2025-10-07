using AutoMapper;
using FiapGamesService.Application.DTOs;
using FiapGamesService.Application.Mappings;
using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Enums;
using FiapGamesService.Domain.Interfaces;
using FiapGamesService.Domain.Models;
using FiapGamesService.Infrastructure.Search;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using System.Net;
using FiapGamesService.Application.Payments;

namespace FiapGamesService.Application.Services
{
    public class GameService
    {
        private readonly IGameCreatedEventRepository _createdRepo;
        private readonly IGameChangedEventRepository _changedRepo;
        private readonly IPaymentsClient _paymentsClient;
        private readonly IMapper _mapper;
        private readonly IElasticClient _es;

        public GameService(
            IGameCreatedEventRepository createdRepo,
            IGameChangedEventRepository changedRepo,
            IPaymentsClient paymentsClient,
            IMapper mapper,
            IElasticClient es)
        {
            _createdRepo = createdRepo;
            _changedRepo = changedRepo;
            _paymentsClient = paymentsClient;
            _mapper = mapper;
            _es = es;
        }

        public async Task<int> CreateAsync(GameCreateDto dto, CancellationToken ct = default)
        {
            var created = _mapper.Map<GameCreatedEvent>(dto);

            await _createdRepo.AddAsync(created);

            var current = await GetByIdAsync(created.Id, ct);
            if (current != null)
            {
                await _es.IndexAsync(new GameSearchDocument
                {
                    Id = current.Id,
                    Name = current.Name,
                    Description = current.Description,
                    Genre = current.Genre,
                    Price = current.Price,
                    CreatedAt = current.CreatedAt
                }, ct);
            }

            return created.Id;
        }

        public async Task UpdateAsync(int gameId, GameUpdateDto dto, CancellationToken ct = default)
        {
            var created = await _createdRepo.GetFirstOrDefaultByConditionAsync(c => c.Id == gameId);
            if (created is null) throw new KeyNotFoundException("Game not found");

            var last = (await _changedRepo.GetListByConditionAsync(c => c.GameId == gameId))
                       .OrderByDescending(c => c.ChangedAt)
                       .FirstOrDefault();

            var current = _mapper.Map<GameDto>(new GameState { Created = created, LastChange = last });

            var changed = _mapper.Map<GameChangedEvent>(dto, opt =>
            {
                opt.Items["GameId"] = gameId;
                opt.Items["Current"] = current;
            });

            await _changedRepo.AddAsync(changed);

            var final = await GetByIdAsync(gameId, ct);
            if (final != null)
            {
                await _es.IndexAsync(new GameSearchDocument
                {
                    Id = final.Id,
                    Name = final.Name,
                    Description = final.Description,
                    Genre = final.Genre,
                    Price = final.Price,
                    CreatedAt = final.CreatedAt
                }, ct);
            }
        }

        public async Task DeleteAsync(int gameId, CancellationToken ct = default)
        {
            var created = await _createdRepo.GetFirstOrDefaultByConditionAsync(c => c.Id == gameId);
            if (created is null) return;

            await _changedRepo.AddAsync(new GameChangedEvent
            {
                GameId = gameId,
                ChangeType = GameChangeType.Deleted,
                ChangedAt = DateTime.UtcNow
            });

            await _es.DeleteAsync(gameId, ct);
        }

        public async Task<GameDto?> GetByIdAsync(int gameId, CancellationToken ct = default)
        {
            var created = await _createdRepo.GetFirstOrDefaultByConditionAsync(c => c.Id == gameId);
            if (created is null) return null;

            var last = (await _changedRepo.GetListByConditionAsync(c => c.GameId == gameId))
                       .OrderByDescending(c => c.ChangedAt)
                       .FirstOrDefault();

            if (last?.ChangeType == GameChangeType.Deleted) return null;

            return _mapper.Map<GameDto>(new GameState { Created = created, LastChange = last });
        }

        public async Task<PaginationResult<GameDto>> SearchEsAsync(
            string? q, string? genre, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            var (docs, total) = await _es.SearchAsync(q, genre, page, pageSize, ct);
            var items = docs.Select(d => new GameDto(d.Id, d.Name, d.Description, d.Price, d.Genre, d.CreatedAt)).ToList();

            return new PaginationResult<GameDto>(
                page,
                (int)Math.Ceiling((double)total / pageSize),
                pageSize,
                (int)total,
                items
            );
        }

        public Task<Dictionary<string, long>> TopGenresAsync(int size = 10, CancellationToken ct = default)
            => _es.TopGenresAsync(Math.Min(size, 50), ct);

        public async Task<(bool ok, object body, int status)> PurchaseAsync(int gameId, PurchaseRequest req, CancellationToken ct = default)
        {
            var game = await GetByIdAsync(gameId, ct);
            if (game is null)
                return (false, new { error = "Jogo não encontrado." }, (int)HttpStatusCode.NotFound);

            const int BRL_ENUM_VALUE = 0;

            var payReq = new PaymentProcessInputForPayments
            {
                UserId = req.UserId,
                GameId = gameId,
                Amount = game.Price,
                Currency = BRL_ENUM_VALUE
            };

            var started = await _paymentsClient.ProcessAsync(payReq, ct);

            if (!started.Success)
            {
                var msg = started.Message ?? "Falha ao iniciar processamento de pagamento.";
                var isConflict = msg.Contains("Já existe um pagamento", StringComparison.OrdinalIgnoreCase);
                return (false, new { error = msg }, isConflict ? (int)HttpStatusCode.Conflict : (int)HttpStatusCode.BadRequest);
            }

            var paymentId = started.Data?.ToString();
            if (string.IsNullOrWhiteSpace(paymentId))
                return (false, new { error = "Payments não retornou o ID da transação." }, (int)HttpStatusCode.BadGateway);

            var (okDetail, detail, detailMsg) = await _paymentsClient.GetByIdAsync(paymentId, ct);

            if (!okDetail || detail is null)
            {
                var fallback = new PurchaseUserResponse(
                    Status: "Processing",
                    Message: started.Message ?? "Processo iniciado.",
                    PaymentId: paymentId,
                    CreatedAt: null,
                    UpdatedAt: null,
                    Game: new { game.Id, game.Name, game.Price }
                );
                return (true, fallback, (int)HttpStatusCode.Accepted);
            }

            var statusText = string.IsNullOrWhiteSpace(detail.Status) ? "Processing" : detail.Status;
            var message = !string.IsNullOrWhiteSpace(detail.Observation)
                ? detail.Observation!
                : (started.Message ?? "Processo finalizado.");

            var http = statusText.Equals("Processed", StringComparison.OrdinalIgnoreCase)
                ? (int)HttpStatusCode.OK
                : statusText.Equals("Failed", StringComparison.OrdinalIgnoreCase)
                    ? (int)HttpStatusCode.BadRequest
                    : (int)HttpStatusCode.Accepted;

            var response = new PurchaseUserResponse(
                Status: statusText,
                Message: message,
                PaymentId: paymentId,
                CreatedAt: detail.CreatedAt,
                UpdatedAt: detail.UpdatedAt,
                Game: new { game.Id, game.Name, game.Price }
            );

            return (true, response, http);
        }

        public async Task<(bool ok, object body, int status)> GetUserGamesAsync(int userId, bool includePending = false, CancellationToken ct = default)
        {
            var (ok, payments, msg) = await _paymentsClient.GetAllByUserAsync(userId, ct);
            if (!ok || payments is null)
                return (false, new { error = msg ?? "Não foi possível consultar os pagamentos do usuário." }, 502);

            var valid = includePending
                ? new[] { "Processed", "Processing" }
                : new[] { "Processed" };

            var latestByGame = payments
                .Where(p => !string.IsNullOrWhiteSpace(p.Status) && valid.Contains(p.Status!, StringComparer.OrdinalIgnoreCase))
                .GroupBy(p => p.GameId)
                .Select(g => g.OrderByDescending(x => x.UpdatedAt).First())
                .ToList();

            if (latestByGame.Count == 0)
                return (true, Array.Empty<UserGameDto>(), 200);

            var result = new List<UserGameDto>();
            foreach (var p in latestByGame)
            {
                var game = await GetByIdAsync(p.GameId, ct);
                if (game is not null)
                {
                    result.Add(new UserGameDto(
                        Id: game.Id,
                        Name: game.Name,
                        Price: game.Price,
                        Status: p.Status ?? "Processed",
                        PurchasedAtUtc: p.UpdatedAt
                    ));
                }
            }

            return (true, result, 200);
        }

        public async Task<List<GameDto>> RecommendAsync(int take = 10, CancellationToken ct = default)
        {
            var top = await TopGenresAsync(size: 3, ct);
            if (top == null || top.Count == 0) return new();

            var primaryGenre = top.OrderByDescending(kv => kv.Value).First().Key;
            var page = await SearchEsAsync(q: null, genre: primaryGenre, page: 1, pageSize: take, ct);
            return page.Data;
        }
    }
}
