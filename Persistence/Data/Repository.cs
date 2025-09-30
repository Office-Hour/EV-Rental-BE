using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Data
{
    public class Repository<T>(ApplicationDbContext applicationDbContext)
        : IRepository<T> where T : class
    {
        private DbSet<T> DbSet => applicationDbContext.Set<T>();
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
        }

        public IQueryable<T> AsQueryable()
        {
            return DbSet.AsQueryable();
        }

        public async Task DeleteAsync<TValue>(TValue id, CancellationToken cancellationToken = default)
        {
            var entity = await DbSet.FindAsync([id], cancellationToken);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }
        public async Task UpdateAsync<TValue>(TValue id, T entity, CancellationToken cancellationToken = default)
        {
            var existingEntity = await DbSet.FindAsync([id], cancellationToken);
            if (existingEntity != null)
            {
                applicationDbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
        }

        public void Update(T entity)
        {
            DbSet.Update(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        public async Task<T> GetByIdAsync<TValue>(TValue id, CancellationToken cancellationToken = default)
        {
            var entity = await DbSet.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with ID {id} not found.");
            return entity;
        }
    }
}
