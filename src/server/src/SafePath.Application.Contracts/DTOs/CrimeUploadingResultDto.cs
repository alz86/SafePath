using System.Collections.Generic;

namespace SafePath.DTOs
{
    //TODO: no longer in use
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
