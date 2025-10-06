using AutoMapper;
using FiapGamesService.Application.DTOs;
using FiapGamesService.Application.Mappings;
using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Enums;
using FiapGamesService.Domain.Interfaces;
using FiapGamesService.Domain.Models;
using FiapGamesService.Infrastructure.Search;

namespace FiapGamesService.Application.Services
{
    public class GameService
    {
        private readonly IGameCreatedEventRepository _createdRepo;
        private readonly IGameChangedEventRepository _changedRepo;
        private readonly IMapper _mapper;
        private readonly IElasticClient _es;

        public GameService(
            IGameCreatedEventRepository createdRepo,
            IGameChangedEventRepository changedRepo,
            IMapper mapper,
            IElasticClient es)
        {
            _createdRepo = createdRepo;
            _changedRepo = changedRepo;
            _mapper = mapper;
            _es = es;
        }

        public async Task<Guid> CreateAsync(GameCreateDto dto, CancellationToken ct = default)
        {
            var created = _mapper.Map<GameCreatedEvent>(dto, opts =>
            {
                opts.Items["GameId"] = Guid.NewGuid();
            });

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

        public async Task UpdateAsync(Guid gameId, GameUpdateDto dto, CancellationToken ct = default)
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

        public async Task DeleteAsync(Guid gameId, CancellationToken ct = default)
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

        public async Task<GameDto?> GetByIdAsync(Guid gameId, CancellationToken ct = default)
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
    }
}
