using SafePath.Entities.FastStorage;
using System.Collections.Generic;

namespace SafePath.Repositories.FastStorage
{
    public interface ISafetyScoreElementRepository : IFastStorageRepositoryBase<SafetyScoreElement>
    {
        void DeleteMany(IEnumerable<int> scoresToDelete);
        SafetyScoreElement? GetByEdgeId(uint edgeId, bool includeDetails = true);
        IList<SafetyScoreElement> GetByIdList(IEnumerable<int> safetyScoresToCheckc, bool includeDetails = true);
        float? GetScoreByEdgeId(uint edgeId);

    }
}
