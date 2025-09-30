using FiapGamesService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FiapGamesService.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetListByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetFirstOrDefaultByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
