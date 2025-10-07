using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CriminalSurveillanceAPI.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(object id);
        Task<IEnumerable<T>> GetAllAsync();
        IQueryable<T> GetQueryable();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task SaveChangesAsync();
    }
}