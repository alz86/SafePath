using SafePath.Entities.FastStorage;
using System;

namespace SafePath.Classes
{
    public class MapElementUpdateDto
    {
        public Guid AreaId { get; set; }

        public int? ElementId { get; set; }

        public double Lat { get; set; }

        public double Lng { get; set; }

        public uint? EdgeId { get; set; }

        public uint? VertexId { get; set; }

        public SecurityElementTypes ElementType { get; set; }
    }
}
