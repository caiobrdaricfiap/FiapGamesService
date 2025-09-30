using FiapGamesService.Domain.Entities;
using FiapGamesService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _db;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetListByConditionAsync(Expression<Func<T, bool>> predicate) =>
            await _db.AsNoTracking().Where(predicate).ToListAsync();

        public async Task<T?> GetFirstOrDefaultByConditionAsync(Expression<Func<T, bool>> predicate) =>
            await _db.AsNoTracking().Where(predicate).FirstOrDefaultAsync();

        public async Task<T?> GetAsync(Guid id) =>
            await _db.FindAsync(id);

        public async Task<T> AddAsync(T entity)
        {
            _db.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var current = await _db.FindAsync(entity.Id);
            if (current is null)
                return null!;

            _context.Entry(current).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var current = await _db.FindAsync(id);
            if (current is null)
                return false;

            _db.Remove(current);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
