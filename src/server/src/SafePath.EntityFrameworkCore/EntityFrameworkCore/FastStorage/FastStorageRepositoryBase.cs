using SafePath.Repositories.FastStorage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class FastStorageRepositoryBase<TEntity, TDbContext> : IFastStorageRepositoryBase<TEntity>
        where TEntity : class
        where TDbContext : SqliteDbContext
    {
        public FastStorageRepositoryBase(TDbContext dbContext)
        {
            DbContext = dbContext;
        }

        protected TDbContext DbContext { get; init; }

        public async Task<TEntity> GetByIdAsync(object id) => await DbContext.Set<TEntity>().FindAsync(id);

        public Task<List<TEntity>> GetAllAsync() => DbContext.Set<TEntity>().ToListAsync();

        public async Task InsertAsync(TEntity entity)
        {
            await DbContext.Set<TEntity>().AddAsync(entity);
        }

        public async Task InsertManyAsync(IEnumerable<TEntity> elements)
        {
            if (elements == null || !elements.Any())
                throw new ArgumentException("No elements provided to insert.", nameof(elements));

            await DbContext.Set<TEntity>().AddRangeAsync(elements);
        }

        public void Update(TEntity entity)
        {
            DbContext.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            DbContext.Set<TEntity>().Remove(entity);
        }

        public Task<int> SaveChangesAsync() => DbContext.SaveChangesAsync();
    }
}
