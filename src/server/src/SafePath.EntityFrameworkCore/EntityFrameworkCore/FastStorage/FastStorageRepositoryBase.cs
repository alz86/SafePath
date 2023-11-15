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

        public TEntity? GetById(object id) => DbContext.Set<TEntity>().Find(id);

        public List<TEntity> GetAll() => DbContext.Set<TEntity>().ToList();

        public void Insert(TEntity entity) => DbContext.Set<TEntity>().Add(entity);

        public void InsertMany(IEnumerable<TEntity> elements)
        {
            if (elements == null || !elements.Any())
                throw new ArgumentException("No elements provided to insert.", nameof(elements));

            DbContext.Set<TEntity>().AddRange(elements);
        }

        public void Update(TEntity entity) => DbContext.Set<TEntity>().Update(entity);

        public void Delete(TEntity entity) => DbContext.Set<TEntity>().Remove(entity);

        public async Task<int> SaveChangesAsync()
        {
            var result = await DbContext.SaveChangesAsync();

            //with this command we force the commit of the data from
            //the .wal file to the main one.
            await DbContext.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint;");
            
            return result;
        }
    }
}