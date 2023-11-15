using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafePath.Repositories.FastStorage
{
    public interface IFastStorageRepositoryBase<TEntity>
        where TEntity : class
    {
        TEntity? GetById(object id);
        List<TEntity> GetAll();
        void Insert(TEntity entity);
        void InsertMany(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<int> SaveChangesAsync();
    }
}
