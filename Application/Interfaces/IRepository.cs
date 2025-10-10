using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync<TValue>(TValue id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync<TValue>(TValue id, T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync<TValue>(TValue id, CancellationToken cancellationToken = default);
        IQueryable<T> AsQueryable();
        void Update(T entity);
    }
}
