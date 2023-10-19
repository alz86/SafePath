using System.Collections.Generic;

namespace SafePath.DTOs
{
    public class CrimeUploadingResultDto
    {
        public bool Error { get; set; }

        public IDictionary<int, CrimeEntryValidationResult>? ValidationErrors { get; set; }
    }

    public enum CrimeEntryValidationResult
    {
        Valid = 0,
        InvalidAddress,
        InvalidSeverity,
    }

}
