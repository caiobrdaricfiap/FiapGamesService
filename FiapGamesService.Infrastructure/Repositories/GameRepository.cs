using FIAP.Games.Domain.Entities;
using FIAP.Games.Domain.Interfaces;
using FIAP.Games.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Games.Infrastructure.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _db;
        public GameRepository(AppDbContext db) => _db = db;

        public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);

        public async Task<PaginationResult<Game>> ListAsync(
            string? search = null,
            string? genre = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var games = _db.Games.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                games = games.Where(g =>
                    EF.Functions.Like(g.Name, $"%{s}%") ||
                    (g.Description != null && EF.Functions.Like(g.Description, $"%{s}%")));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                var gtrim = genre.Trim();                
                games = games.Where(x => x.Genre == gtrim);
            }

            games = games.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Name);

            return await games.GetPaged(page, pageSize, ct);  
        }

        public async Task AddAsync(Game game, CancellationToken ct = default)
        {
            await _db.Games.AddAsync(game, ct);
        }

        public Task UpdateAsync(Game game, CancellationToken ct = default)
        {
            _db.Games.Update(game);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.Games.FirstOrDefaultAsync(g => g.Id == id, ct);
            if (entity is null) return;
            _db.Games.Remove(entity);
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
