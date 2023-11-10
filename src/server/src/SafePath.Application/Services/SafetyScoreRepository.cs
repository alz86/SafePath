using Itinero.SafePath;
using SafePath.Repositories.FastStorage;

namespace SafePath.Services
{
    public class SafetyScoreRepository : ISafetyScoreRepository
    {
        private readonly ISafetyScoreElementRepository safetyScoreRepository;

        public SafetyScoreRepository(ISafetyScoreElementRepository safetyScoreRepository)
        {
            this.safetyScoreRepository = safetyScoreRepository;
        }

        public float? GetScore(uint edgeId) => safetyScoreRepository.GetScoreByEdgeId(edgeId);
    }
}