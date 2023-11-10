namespace SafePath.Entities.FastStorage
{
    public class SafetyScoreElementMapElement
    {
        public int SafetyScoreElementId { get; set; }
        public SafetyScoreElement SafetyScoreElement { get; set; }

        public int MapElementId { get; set; }
        public MapElement MapElement { get; set; }
    }

}
