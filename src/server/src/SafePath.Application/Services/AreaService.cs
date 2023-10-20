using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using SafePath.Classes;
using SafePath.DTOs;
using SafePath.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Area = SafePath.Entities.Area;

namespace SafePath.Services
{
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    [Authorize()]
    public class AreaService : SafePathAppService, IAreaService
    {
        private readonly IItineroProxy proxy;
        private readonly IMapper mapper;
        private readonly IRepository<Area, Guid> areaRepository;
        private readonly IRepository<CrimeDataUploading, Guid> crimeDataUploadingRepository;
        private readonly IRepository<CrimeDataUploadingEntry, Guid> crimeDataUploadingEntryRepository;

        private IList<MapSecurityElementDto>? securityElements;
        private GeoJsonFeatureCollection? mapLibreGeoJSON;

        public AreaService(IMapper mapper, IItineroProxy proxy, IRepository<Area, Guid> areaRepository,
            IRepository<CrimeDataUploading, Guid> crimeDataUploadingRepository, IRepository<CrimeDataUploadingEntry, Guid> crimeDataUploadingEntryRepository)
        {
            this.mapper = mapper;
            this.proxy = proxy;
            this.areaRepository = areaRepository;
            this.crimeDataUploadingRepository = crimeDataUploadingRepository;
            this.crimeDataUploadingEntryRepository = crimeDataUploadingEntryRepository;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task<IList<AreaDto>> GetAdminAreas()
        {
            var entities = (await areaRepository.GetQueryableAsync())
                                    .WhereIf(CurrentTenant != null, a => a.TenantId == CurrentTenant!.Id)
                                    .ToImmutableList();

            return mapper.Map<IList<AreaDto>>(entities);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public Task<IList<MapSecurityElementDto>> GetSecurityElements()
        {
            securityElements ??= mapper.Map<IList<MapSecurityElementDto>>(proxy.SecurityElements);
            return Task.FromResult(securityElements);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON()
        {
            mapLibreGeoJSON ??= proxy.SecurityLayerGeoJSON;
            return Task.FromResult(mapLibreGeoJSON);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task<CrimeUploadingResultDto> UploadCrimeReportCSV(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new UserFriendlyException("The selected file to upload is empty.");

            if (fileContent.Length > Constants.MaxCsvFileSize)
                throw new UserFriendlyException("The selected file to upload is too big. The maximum size allowed is 50MB.");

            var entries = ReadCrimeData(fileContent);

            var validationResult = ValidateCrimeEntries(entries);

            if (validationResult.Count > 0)
            {
                //there are errors, the import is not done.
                return new CrimeUploadingResultDto { Success = false, ValidationErrors = validationResult };
            }

            //data is valid, let's save it in the DB.
            var uploadingEntity = new CrimeDataUploading { RawData = fileContent, TenantId = CurrentTenant!.Id };
            uploadingEntity = await crimeDataUploadingRepository.InsertAsync(uploadingEntity);

            var dbEntries = entries.Select(e => new CrimeDataUploadingEntry
            {
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Severity = e.Severity,
                CrimeDataUploading = uploadingEntity
            });
            await crimeDataUploadingEntryRepository.InsertManyAsync(dbEntries);

            //TODO: regenerate safety info

            return new CrimeUploadingResultDto { Success = true };
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

                var edge = proxy.GetItineroEdgeIds(entry.Latitude, entry.Longitude);
                if (edge.Error)
                {
                    result = CrimeEntryValidationResult.InvalidAddress;
                }

                if (entry.Severity < 1 || entry.Severity > 5)
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
