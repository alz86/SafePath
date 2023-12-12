using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SafePath.Services
{
    public interface ISafetyScoreChangeTrackerFactory
    {
        ISafetyScoreChangeTracker Create();
    }

    public class SafetyScoreChangeTrackerFactory : ISafetyScoreChangeTrackerFactory
    {
        private readonly ISafetyScoreCalculator safetyScoreCalculator;
        private readonly ISafetyScoreElementRepository safetyScoreElementRepository;

        public SafetyScoreChangeTrackerFactory(ISafetyScoreCalculator safetyScoreCalculator, ISafetyScoreElementRepository safetyScoreElementRepository)
        {
            this.safetyScoreCalculator = safetyScoreCalculator;
            this.safetyScoreElementRepository = safetyScoreElementRepository;
        }

        public ISafetyScoreChangeTracker Create() => new SafetyScoreChangeTracker(safetyScoreCalculator, safetyScoreElementRepository);

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


            public void Track(params SafetyScoreElement?[] elements) =>
                elements?.Where(e => e != null).ToList().ForEach(e => elementsToTrack.Add(e!.Id));

            public void Track(params MapElement?[] elements) => elements?.ToList().ForEach(Track);


            public void Track(MapElement? element)
            {
                if (element?.SafetyScoreElements?.Any() != true) return;
                foreach (var score in element.SafetyScoreElements)
                    elementsToTrack.Add(score.Id);
            }

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