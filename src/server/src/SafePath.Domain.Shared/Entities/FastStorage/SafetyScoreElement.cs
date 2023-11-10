using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SafePath.Entities.FastStorage
{
    public class SafetyScoreElement
    {
        public SafetyScoreElement()
        {
            MapElements = new HashSet<MapElement>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public float Score { get; set; }

        public uint EdgeId { get; set; }

        public ICollection<MapElement> MapElements { get; set; }
    }
}
