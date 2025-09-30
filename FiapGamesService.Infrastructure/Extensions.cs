using FiapGamesService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FiapGamesService.Infrastructure
{
    public static class Extensions
    {
        public static async Task<PaginationResult<T>> GetPaged<T>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken ct = default) where T : class
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var total = await query.CountAsync(ct);
            var totalPages = (int)Math.Ceiling((double)total / pageSize);
            var skip = (page - 1) * pageSize;

            var data = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginationResult<T>(page, totalPages, pageSize, total, data);
        }
    }
}
