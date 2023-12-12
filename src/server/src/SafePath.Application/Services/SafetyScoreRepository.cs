using Itinero.SafePath;
using SafePath.Repositories.FastStorage;

namespace SafePath.Services
{

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class SafetyScoreRepository : ISafetyScoreRepository
    {
        private readonly ISafetyScoreElementRepository safetyScoreRepository;

        public SafetyScoreRepository(ISafetyScoreElementRepository safetyScoreRepository)
        {
            this.safetyScoreRepository = safetyScoreRepository;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public float? GetItineroRouteScore(uint edgeId)
        {
            var score = safetyScoreRepository.GetScoreByEdgeId(edgeId);
            return score != null ? SafetyScoreToItineroWeight(score.Value) : null;
        }

        private static float SafetyScoreToItineroWeight(double safetyScore)
        {
            return (float)(2 - safetyScore);
            /*
            if (safetyScore == 0) return 0;

            if (safetyScore < 1) safetyScore += 1;

            return 1 / safetyScore;
            */
        }

    }
}