using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SafePath.Entities.FastStorage
{
    public class MapElement
    {
        public MapElement()
        {
            SafetyScoreElements = new HashSet<SafetyScoreElement>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lng { get; set; }

        public long? OSMNodeId { get; set; }

        public uint? EdgeId { get; set; }
        public uint? VertexId { get; set; }

        [MaxLength(1000)]
        public string? ItineroMappingError { get; set; }

        public SecurityElementTypes Type { get; set; }

        public ICollection<SafetyScoreElement> SafetyScoreElements { get; set; }
    }
}
