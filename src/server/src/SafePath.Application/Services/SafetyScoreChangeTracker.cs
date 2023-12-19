using SafePath.Entities.FastStorage;

namespace SafePath.Services
{
    /// <summary>
    /// Represents a class able to track <see cref="SafetyScoreElement"/> 
    /// entities in order to recalculate its Safety Score when <see cref="MapElement"/>
    /// associated with it change.
    /// </summary>
    public interface ISafetyScoreChangeTracker
    {
        /// <summary>
        /// Adds the supplied list of <see cref="SafetyScoreElement"/> to the list of 
        /// tracked elements.
        /// </summary>
        /// <param name="element"></param>
        void Track(params SafetyScoreElement?[] element);

        /// <summary>
        /// Adds the <see cref="SafetyScoreElement"/> associated
        /// to the supplied list of Map elements to the list of tracked
        /// entities.
        /// </summary>
        void Track(params MapElement?[] element);

        /// <summary>
        /// Anlyzes the list of tracked elements and recalculates
        /// its safety score, saving the information to the safety
        /// score database.
        /// </summary>
        /// <returns>
        /// Amount of entities which safety score was recalculated,
        /// it changed with respect to the previous one, and this
        /// new value was saved in the DB
        /// </returns>
        int UpdateScores();
    }
}