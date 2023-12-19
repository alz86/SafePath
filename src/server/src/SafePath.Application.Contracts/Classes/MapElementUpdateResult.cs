namespace SafePath.Classes
{
    public class MapElementUpdateResult
    {
        public int? MapElementId { get; set; }
        public ResultValues Result { get; set; }

        public enum ResultValues
        {
            Error,
            NotFound,
            NoChanges,
            PointNotInMap,
            DuplicatesElement,
            Success
        }
    }
}