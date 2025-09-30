using AutoMapper;
using FiapGamesService.Application.DTOs;
using FiapGamesService.Application.Mappings;
using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Enums;
using FiapGamesService.Domain.Interfaces;
using FiapGamesService.Domain.Models;

namespace FiapGamesService.Application.Services
{
    public class GameService
    {
        private readonly IGameCreatedEventRepository _createdRepo;
        private readonly IGameChangedEventRepository _changedRepo;
        private readonly IMapper _mapper;

        public GameService(IGameCreatedEventRepository createdRepo, IGameChangedEventRepository changedRepo, IMapper mapper)
        {
            _createdRepo = createdRepo;
            _changedRepo = changedRepo;
            _mapper = mapper;
        }

        public async Task<Guid> CreateAsync(GameCreateDto dto, CancellationToken ct = default)
        {
            var created = _mapper.Map<GameCreatedEvent>(dto, opts =>
            {
                opts.Items["GameId"] = Guid.NewGuid();
            });

            await _createdRepo.AddAsync(created);
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

        public async Task<PaginationResult<GameDto>> ListAsync(
            string? search, string? genre, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var createdList = await _createdRepo.GetListByConditionAsync(c =>
                (string.IsNullOrEmpty(search) ||
                    (c.Name.Contains(search) || (c.Description != null && c.Description.Contains(search)))) &&
                (string.IsNullOrEmpty(genre) || c.Genre == genre));

            var createdArr = createdList.ToArray();
            if (createdArr.Length == 0)
                return new PaginationResult<GameDto>(page, 0, pageSize, 0, new List<GameDto>());

            var ids = createdArr.Select(c => c.Id).ToHashSet();

            var allChanges = await _changedRepo.GetListByConditionAsync(ch => ids.Contains(ch.GameId));
            var lastByGame = allChanges
                .GroupBy(ch => ch.GameId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ChangedAt).First());

            var states = new List<GameState>(createdArr.Length);
            foreach (var c in createdArr)
            {
                lastByGame.TryGetValue(c.Id, out var last);
                if (last?.ChangeType == GameChangeType.Deleted) continue;

                states.Add(new GameState { Created = c, LastChange = last });
            }

            var dtos = _mapper.Map<List<GameDto>>(states);

            dtos = dtos.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Name).ToList();

            var total = dtos.Count;
            var data = dtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PaginationResult<GameDto>(page,
                (int)Math.Ceiling((double)total / pageSize),
                pageSize,
                total,
                data);
        }
    }
}
