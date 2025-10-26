using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Persistence.Data
{
    public class UnitOfWork(ApplicationDbContext applicationDbContext)
        : IUnitOfWork
    {
        private IDbContextTransaction? _currentTransaction;
        private readonly Dictionary<Type, object> _repositories = [];
        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories.TryGetValue(typeof(T), out var repository))
            {
                return (IRepository<T>)repository;
            }
            var newRepository = new Repository<T>(applicationDbContext);
            _repositories.Add(typeof(T), newRepository);
            return newRepository;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return applicationDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }
            _currentTransaction = await applicationDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                _currentTransaction?.Commit();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _currentTransaction?.RollbackAsync()!;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }
    }
}
