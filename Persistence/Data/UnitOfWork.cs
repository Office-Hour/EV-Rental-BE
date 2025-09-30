using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Persistence.Data
{
    public class UnitOfWork(ApplicationDbContext applicationDbContext)
        : IUnitOfWork
    {
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
    }
}
