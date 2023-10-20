using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace SafePath.Entities
{
    /// <summary>
    /// Represents every entry on a crime data uploading,
    /// with the information of a particular crime rate to
    /// be set in a particular location.
    /// </summary>
    public class CrimeDataUploadingEntry : Entity<Guid>
    {
        /// <summary>
        /// Parent entity representing the data uploading
        /// where this entry was sent.
        /// </summary>
        [Required]
        public CrimeDataUploading CrimeDataUploading { get; set; }

        /// <summary>
        /// Latitude associated to the entry
        /// </summary>
        [Required, Range(-90, 90)]
        public float Latitude { get; set; }

        /// <summary>
        /// Longitude associated to the entry
        /// </summary>
        [Required, Range(-180, 180)]
        public float Longitude { get; set; }

        /// <summary>
        /// Address associated to the entry
        /// </summary>
        /// <remarks>
        /// When uploading information, the user can opt
        /// between including the coordinated (Lat/Lon) or
        /// an address to be resolved by the system. If choose
        /// this last option, here the address provided by
        /// the user is saved
        /// </remarks>
        public string? Address { get; set; }

        /// <summary>
        /// Severity of the crime rate associated to the entry
        /// </summary>
        [Required, Range(1, 5)]
        public float Severity { get; set; }
    }
}
