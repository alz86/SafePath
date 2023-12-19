using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Collections.Concurrent;
using System.Linq;

namespace SafePath.Services
{
    /// <summary>
    /// Represents an object able to create
    /// <see cref="ISafetyScoreChangeTracker"/> entities.
    /// </summary>
    public interface ISafetyScoreChangeTrackerFactory
    {
        /// <summary>
        /// Creates a new <see cref="ISafetyScoreChangeTracker"/> entity.
        /// </summary>
        /// <returns></returns>
        ISafetyScoreChangeTracker Create();
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class SafetyScoreChangeTrackerFactory : ISafetyScoreChangeTrackerFactory
    {
        private readonly ISafetyScoreCalculator safetyScoreCalculator;
        private readonly ISafetyScoreElementRepository safetyScoreElementRepository;

        public SafetyScoreChangeTrackerFactory(ISafetyScoreCalculator safetyScoreCalculator, ISafetyScoreElementRepository safetyScoreElementRepository)
        {
            this.safetyScoreCalculator = safetyScoreCalculator;
            this.safetyScoreElementRepository = safetyScoreElementRepository;
        }


        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public ISafetyScoreChangeTracker Create() => new SafetyScoreChangeTracker(safetyScoreCalculator, safetyScoreElementRepository);


        /// <summary>
        /// <inheritdoc />
        /// </summary>
        private class SafetyScoreChangeTracker : ISafetyScoreChangeTracker
        {
            private readonly BlockingCollection<int> elementsToTrack = [];
            private readonly ISafetyScoreCalculator safetyScoreCalculator;
            private readonly ISafetyScoreElementRepository safetyScoreElementRepository;

            public SafetyScoreChangeTracker(ISafetyScoreCalculator safetyScoreCalculator, ISafetyScoreElementRepository safetyScoreElementRepository)
            {
                this.safetyScoreCalculator = safetyScoreCalculator;
                this.safetyScoreElementRepository = safetyScoreElementRepository;
            }


            /// <summary>
            /// <inheritdoc />
            /// </summary>
            public void Track(params SafetyScoreElement?[] elements) =>
                elements?.Where(e => e != null).ToList().ForEach(e => elementsToTrack.Add(e!.Id));

            /// <summary>
            /// <inheritdoc />
            /// </summary>
            public void Track(params MapElement?[] elements) => elements?.ToList().ForEach(Track);


            private void Track(MapElement? element)
            {
                if (element?.SafetyScoreElements?.Any() != true) return;
                foreach (var score in element.SafetyScoreElements)
                    elementsToTrack.Add(score.Id);
            }

            /// <summary>
            /// <inheritdoc />
            /// </summary>
            public int UpdateScores()
            {
                if (elementsToTrack.Count == 0) return 0;

                var updateCounter = 0;
                var elements = safetyScoreElementRepository.GetByIdList(elementsToTrack.Distinct(), true);
                foreach (var entry in elements)
                {
                    if (entry.MapElements.Count == 0)
                    {
                        safetyScoreElementRepository.Delete(entry);
                        continue;
                    }

                    var current = entry.Score;
                    var newScore = safetyScoreCalculator.Calculate(entry.MapElements);

                    if (current != newScore)
                    {
                        updateCounter++;
                        entry.Score = newScore;
                        safetyScoreElementRepository.Update(entry);
                    }
                }

                return updateCounter;
            }
        }
    }
}