using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SafePath.Entities
{
    /// <summary>
    /// Represents a physical area mapped in the system,
    /// ir oder to be able to generate safe routes to navigate
    /// through it.
    /// </summary>
    /// <remarks>
    /// This entity typically will represent a city or district,
    /// but technically can be from a discrete portion o 1x1 km in
    /// the map to a whole continent.
    /// </remarks>
    public class Area : FullAuditedEntity<Guid>, IMultiTenant
    {

        protected Area() : base() { }

        public Area(Guid id) : base(id) { }

        /// <summary>
        /// Gets or sets the friendly name to display
        /// to the users
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// Tenant associated to this area.
        /// </summary>
        public Guid? TenantId { get; set; }

        //TODO: create new entity to hold versions of the
        //files imported and processed

        /// <summary>
        /// Gets or sets the URL from where the OSM file
        /// was downloaded.
        /// </summary>
        [MaxLength(2000)]
        public string? OsmFileUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the data
        /// was already imported to the system or not.
        /// </summary>
        [DefaultValue(false)]
        public bool OsmDataImported { get; set; } = false;

        /// <summary>
        /// Gets or sets the default initial latitude 
        /// associated to this area.
        /// </summary>
        /// <remarks>
        /// This value is used to make the app more user-friendly
        /// centering the map in this location so it is easier for
        /// the user to work on it.
        /// </remarks>
        [Required, Range(-90, 90)]
        public double InitialLatitude { get; set; }


        /// <summary>
        /// Gets or sets the default initial longitude
        /// associated to this area.
        /// </summary>
        /// <remarks>
        /// This value is used to make the app more user-friendly
        /// centering the map in this location so it is easier for
        /// the user to work on it.
        /// </remarks>
        [Required, Range(-180, 180)]
        public double InitialLongitude { get; set; }
    }
}
