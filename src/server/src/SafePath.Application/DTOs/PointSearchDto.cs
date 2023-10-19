namespace SafePath.DTOs
{
    public class PointSearchDto
    {
        public bool Error { get; set; } = false;
        public string? ErrorMessaege { get; set; }

        public uint EdgeId { get; set; }

        public uint VertexId { get; set; }
    }
}
