using SafePath.Entities.FastStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafePath.Repositories.FastStorage
{
    public interface IFastStorageRepositoryBase<TEntity>
    where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id);
        Task<List<TEntity>> GetAllAsync();
        Task InsertAsync(TEntity entity);
        Task InsertManyAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<int> SaveChangesAsync();

    }

}
