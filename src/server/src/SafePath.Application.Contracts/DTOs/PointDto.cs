using SafePath.Entities;
using System.ComponentModel.DataAnnotations;

namespace SafePath.DTOs
{
    public class PointDto
    {
        [Required]
        public CoordinatesDto Coordinates { get; set; }

        [Required]
        public SecurityElementTypesDto Type { get; set; }

        public int? MapElementId { get; set; }
    }
}
