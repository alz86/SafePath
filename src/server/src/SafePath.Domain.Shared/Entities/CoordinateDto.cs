using System;
using System.ComponentModel.DataAnnotations;

namespace SafePath.Entities
{
    public class CoordinatesDto
    {
        /// <summary>
        /// Latitude associated to the entry
        /// </summary>
        [Required, Range(-90, 90)]
        public double Lat { get; set; }

        /// <summary>
        /// Longitude associated to the entry
        /// </summary>
        [Required, Range(-180, 180)]
        public double Lng { get; set; }

    }
}
