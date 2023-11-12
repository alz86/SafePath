using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class MapElementRepository : FastStorageRepositoryBase<MapElement, SqliteDbContext>, IMapElementRepository
    {
        public MapElementRepository(SqliteDbContext dbContext) : base(dbContext)
        {
        }
    }
}
