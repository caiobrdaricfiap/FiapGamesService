using FIAP.Games.Domain.Entities;
using FIAP.Games.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Games.Domain.Interfaces
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<PaginationResult<Game>> ListAsync(string? search = null, string? genre = null, int page = 1, int pageSize = 20, CancellationToken ct = default);

        Task AddAsync(Game game, CancellationToken ct = default);
        Task UpdateAsync(Game game, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
