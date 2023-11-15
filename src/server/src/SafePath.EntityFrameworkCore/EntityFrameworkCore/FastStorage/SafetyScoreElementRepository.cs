using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class SafetyScoreElementRepository : FastStorageRepositoryBase<SafetyScoreElement, SqliteDbContext>, ISafetyScoreElementRepository
    {
        public SafetyScoreElementRepository(SqliteDbContext dbContext) : base(dbContext)
        {
        }

        public SafetyScoreElement? GetByEdgeId(uint edgeId, bool includeDetails = true)
        {
            var query = DbContext.SafetyScoreElements
                .Where(element => element.EdgeId == edgeId);

            if (includeDetails) query = query.Include(q => q.MapElements);

            return query.FirstOrDefault();
        }

        public float? GetScoreByEdgeId(uint edgeId) =>
            DbContext.SafetyScoreElements
                .Where(element => element.EdgeId == edgeId)
                .FirstOrDefault()?.Score;

        public IList<SafetyScoreElement> GetByIdList(IEnumerable<int> safetyScoresToCheck, bool includeEntities = true)
        {
            var query = DbContext.SafetyScoreElements
                .Where(s => safetyScoresToCheck.Contains(s.Id));

            if (includeEntities) query = query.Include(q => q.MapElements);

            return query.ToList();
        }

        public void DeleteMany(IEnumerable<int> scoresToDelete) =>
            DbContext.SafetyScoreElements.Where(s => scoresToDelete.Contains(s.Id)).ExecuteDelete();

    }
}
