using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Linq.Dynamic.Core;
using System.Linq;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class SafetyScoreElementRepository : FastStorageRepositoryBase<SafetyScoreElement, SqliteDbContext>, ISafetyScoreElementRepository
    {
        public SafetyScoreElementRepository(SqliteDbContext dbContext) : base(dbContext)
        {
        }

        public float? GetScoreByEdgeId(uint edgeId) =>
            DbContext.SafetyScoreElements
                .Where(element => element.EdgeId == edgeId)
                .FirstOrDefault()?.Score;
    }
}
