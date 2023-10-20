namespace SafePath.DTOs
{
    /// <summary>
    /// Class with th result of the
    /// searching for a point on Itinero's map
    /// associated to a particular coordinate.
    /// </summary>
    public class PointSearchDto
    {
        /// <summary>
        /// Value indicating whether the operation
        /// failed.
        /// </summary>
        public bool Error { get; set; } = false;
        
        /// <summary>
        /// Gets the operations' error message, if any.
        /// </summary>
        public string? ErrorMessaege { get; set; }

        /// <summary>
        /// Gets the EdgeId Itinero associates to 
        /// a particular coordinate.
        /// </summary>
        public uint? EdgeId { get; set; }

        /// <summary>
        /// Gets the VertexId Itinero associates to 
        /// a particular coordinate.
        /// </summary>
        public uint? VertexId { get; set; }
    }
}
