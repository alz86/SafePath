using CsvHelper;
using CsvHelper.Configuration;
using SafePath.Classes;
using SafePath.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SafePath.Services
{
    public interface IDataValidator
    {
        Task ValidateCrimeReportCSVFile(string fileContent, out IList<CrimeEntry> entries);
    }

    public class DataValidator : IDataValidator
    {
        private readonly IItineroProxy itineroProxy;

        public DataValidator(IItineroProxy itineroProxy)
        {
            this.itineroProxy = itineroProxy;
        }

        public Task ValidateCrimeReportCSVFile(string fileContent, out IList<CrimeEntry> entries)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new UserFriendlyException("The selected file to upload is empty.");

            if (fileContent.Length > Constants.MaxCsvFileSize)
                throw new UserFriendlyException("The selected file to upload is too big. The maximum size allowed is 50MB.");

            entries = ReadCrimeData(fileContent);

            var validationResult = ValidateCrimeEntries(entries);
            if (validationResult.Count > 0)
            {
                //there are errors, the import is not done.
                var validationErrors = validationResult.Select(ToValidationError).ToList();
                throw new AbpValidationException("There were errors in the file provided", validationErrors);
            }

            return Task.CompletedTask;
        }

        private ValidationResult ToValidationError(KeyValuePair<int, CrimeEntryValidationResult> pair, int arg2)
        {
            string errorMessage = string.Empty;
            switch (pair.Value)
            {
                case CrimeEntryValidationResult.InvalidAddress:
                    errorMessage = $"The address could not be resolved.";
                    break;
                case CrimeEntryValidationResult.InvalidSeverity:
                    errorMessage = $"The severity value is invalid.";
                    break;
            }
            return new ValidationResult($"Line {pair.Key + 1}: {errorMessage}");
        }

        /// <summary>
        /// Validates a list of <see cref="CrimeEntry"/> entities, checking
        /// if they are valid entries to be processed.
        /// </summary>
        private IDictionary<int, CrimeEntryValidationResult> ValidateCrimeEntries(IList<CrimeEntry> entries)
        {
            var results = new Dictionary<int, CrimeEntryValidationResult>();
            for (int i = 0; i < entries.Count; i++)
            {
                CrimeEntry entry = entries[i];
                CrimeEntryValidationResult? result = null;
                if (entry.Latitude == 0 || entry.Longitude == 0)
                {
                    result = CrimeEntryValidationResult.InvalidAddress;
                }

                var edge = itineroProxy.GetItineroEdgeIds(entry.Latitude, entry.Longitude);
                if (edge.Error)
                {
                    result = CrimeEntryValidationResult.InvalidAddress;
                }

                if (entry.Severity < 0 || entry.Severity > 5)
                {
                    result = CrimeEntryValidationResult.InvalidSeverity;
                }

                if (result != null)
                    results.Add(i, result.Value);
            }

            return results;
        }

        /// <summary>
        /// Reads the content on the supplied CSV and parses to a list
        /// of <see cref="CrimeEntry"/> entities.
        /// </summary>
        private static IList<CrimeEntry> ReadCrimeData(string fileContent)
        {
            using var reader = new StringReader(fileContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            return csv.GetRecords<CrimeEntry>().ToList();
        }
    }
}
