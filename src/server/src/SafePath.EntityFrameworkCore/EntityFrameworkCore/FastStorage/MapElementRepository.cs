using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Linq.Dynamic.Core;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class MapElementRepository : FastStorageRepositoryBase<MapElement, SqliteDbContext>, IMapElementRepository
    {
        public MapElementRepository(SqliteDbContext dbContext) : base(dbContext)
        {
        }

        public MapElement? GetById(int id, bool includeDetails = true)
        {
            var query = DbContext.MapElements.Where(m => m.Id == id);
            if (includeDetails) query = query.Include(q => q.SafetyScoreElements);
            return query.FirstOrDefault();
        }

        public IList<MapElement>? GetByEdgeId(ulong edgeId, bool includeDetails = true)
        {
            var query = DbContext.MapElements.Where(m => m.EdgeId == edgeId);
            if (includeDetails) query = query.Include(q => q.SafetyScoreElements);
            return query.ToList();
        }
    }
}
