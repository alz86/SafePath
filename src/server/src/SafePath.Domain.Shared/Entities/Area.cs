using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SafePath.Entities
{
    public class Area : FullAuditedEntity<Guid>, IMultiTenant
    {

        protected Area() : base() { }

        public Area(Guid id) : base(id)
        {
        }

        [Required]
        public string DisplayName { get; set; }

        public Guid? TenantId { get; set; }

        //TODO: create new entity to hold versions of the
        //files imported and processed

        [MaxLength(2000)]
        public string? OsmFileUrl { get; set; }

        [DefaultValue(false)]
        public bool OsmDataImported { get; set; } = false;

        [Required, Range(-90, 90)]
        public double InitialLatitude { get; set; }

        [Required, Range(-180, 180)]
        public double InitialLongitude { get; set; }
    }
}
