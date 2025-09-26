using FIAP.Games.Application.DTOs;
using FIAP.Games.Domain.Entities;
using FIAP.Games.Domain.Interfaces;
using FIAP.Games.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Games.Application.Services
{
    public class GameService
    {
        private readonly IGameRepository _repo;

        public GameService(IGameRepository repo)
        {
            _repo = repo;
        }

        public async Task<PaginationResult<GameDto>> ListAsync(
            string? search, string? genre, int page, int pageSize, CancellationToken ct = default)
        {
            var result = await _repo.ListAsync(search, genre, page, pageSize, ct);

            var dtoItems = result.Data
                .Select(g => new GameDto(g.Id, g.Name, g.Description, g.Price, g.Genre, g.CreatedAt))
                .ToList();

            return new PaginationResult<GameDto>(
                result.Page,
                result.TotalPages,
                result.PageSize,
                result.ItemsCount,
                dtoItems
            );
        }

        public async Task<GameDto?> GetAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) return null;

            var g = await _repo.GetByIdAsync(id, ct);
            if (g is null) return null;

            return new GameDto(
                id: g.Id,
                name: g.Name,
                description: g.Description,
                price: g.Price,
                genre: g.Genre,
                createdAt: g.CreatedAt
            );
        }

        public async Task<Guid> CreateAsync(GameCreateDto dto, CancellationToken ct = default)
        {
            if (dto.Price < 0) throw new ArgumentOutOfRangeException(nameof(dto.Price));
            var game = Game.Create(dto.Name, dto.Description, dto.Price, dto.Genre);
            await _repo.AddAsync(game, ct);
            await _repo.SaveChangesAsync(ct);
            return game.Id;
        }

        public async Task UpdateAsync(Guid id, GameUpdateDto dto, CancellationToken ct = default)
        {
            var g = await _repo.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException("Game not found");
            g.Update(dto.Name, dto.Description, dto.Price, dto.Genre);
            await _repo.UpdateAsync(g, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _repo.DeleteAsync(id, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
