using SafePath.Entities.FastStorage;

namespace SafePath.Repositories.FastStorage
{
    public interface ISafetyScoreElementRepository : IFastStorageRepositoryBase<SafetyScoreElement>
    {
        float? GetScoreByEdgeId(uint edgeId);

    }
}
