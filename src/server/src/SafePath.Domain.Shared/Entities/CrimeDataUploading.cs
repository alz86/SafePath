using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SafePath.Entities
{
    /// <summary>
    /// Entity representing the an uploading of
    /// crime data to the system
    /// </summary>
    public class CrimeDataUploading : FullAuditedEntity<Guid>, IMultiTenant
    {
        public CrimeDataUploading() : base() { }

        public CrimeDataUploading(Guid id) : base(id) { }

        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the raw data of the file
        /// uploaded.
        /// </summary>
        /// <remarks>
        /// This information is currently stored
        /// only to ease debugging processes.
        /// Should not be used anymore once the
        /// system is in production.    
        /// </remarks>
        public string? RawData { get; set; }

        /// <summary>
        /// Gets the list of entries associated
        /// to this uploading
        /// </summary>
        public IList<CrimeDataUploadingEntry> EntryList { get; set; } = new List<CrimeDataUploadingEntry>();
    }
}
