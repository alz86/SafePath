using System.ComponentModel.DataAnnotations;

namespace SafePath.DTOs
{
    public class CreateAreaInputDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string OSMFileUrl { get; set; }

        [Required, Range(-90, 90)]
        public double Latitude { get; set; }

        [Required, Range(-180, 180)]
        public double Longitude { get; set; }
    }
}
