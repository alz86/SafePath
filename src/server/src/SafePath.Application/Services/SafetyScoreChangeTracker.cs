using SafePath.Entities.FastStorage;

namespace SafePath.Services
{
    public interface ISafetyScoreChangeTracker
    {
        void Track(params SafetyScoreElement?[] element);

        void Track(params MapElement?[] element);

        int UpdateScores();
    }
}