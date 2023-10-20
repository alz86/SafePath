using System.Collections.Generic;

namespace SafePath.DTOs
{
    public class CrimeUploadingResultDto
    {
        public bool Success { get; set; }

        public IDictionary<int, CrimeEntryValidationResult>? ValidationErrors { get; set; }
    }

    public enum CrimeEntryValidationResult
    {
        Valid = 0,
        InvalidAddress,
        InvalidSeverity,
    }

}
