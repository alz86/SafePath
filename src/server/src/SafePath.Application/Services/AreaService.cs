using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using OsmSharp.API;
using SafePath.Classes;
using SafePath.DTOs;
using SafePath.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Area = SafePath.Entities.Area;

namespace SafePath.Services
{
    [Authorize()]
    public class AreaService : SafePathAppService, IAreaService
    {
        private readonly IItineroProxy proxy;
        private readonly IMapper mapper;
        private readonly IRepository<Area, Guid> areaRepository;

        private IList<MapSecurityElementDto>? securityElements;
        private GeoJsonFeatureCollection? mapLibreGeoJSON;

        public AreaService(IMapper mapper, IItineroProxy proxy, IRepository<Area, Guid> areaRepository)
        {
            this.mapper = mapper;
            this.proxy = proxy;
            this.areaRepository = areaRepository;
        }

        public async Task<IList<AreaDto>> GetAdminAreas()
        {
            var entities = (await areaRepository.GetQueryableAsync())
                                    .WhereIf(CurrentTenant != null, a => a.TenantId == CurrentTenant!.Id)
                                    .ToImmutableList();

            return mapper.Map<IList<AreaDto>>(entities);
        }

        public Task<IList<MapSecurityElementDto>> GetSecurityElements()
        {
            securityElements ??= mapper.Map<IList<MapSecurityElementDto>>(proxy.SecurityElements);
            return Task.FromResult(securityElements);
        }

        public Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON()
        {
            mapLibreGeoJSON ??= proxy.SecurityLayerGeoJSON;
            return Task.FromResult(mapLibreGeoJSON);
        }

        public Task<CrimeUploadingResultDto> UploadCrimeReportCSV(string fileContent)
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
                return Task.FromResult(new CrimeUploadingResultDto { Error = true, ValidationErrors = validationResult });
            }

        }

        private IDictionary<int, CrimeEntryValidationResult> ValidateCrimeEntries(IList<CrimeEntry> entries)
        {
            var results = new Dictionary<int, CrimeEntryValidationResult>();
            for (int i = 0; i < entries.Count; i++)
            {
                CrimeEntry entry = entries[i];
                CrimeEntryValidationResult? result = null;
                if (entry.Lat == 0 || entry.Lon == 0)
                {
                    result = CrimeEntryValidationResult.InvalidAddress;
                }

                var edge = proxy.GetItineroEdgeIds(entry.Lat, entry.Lon);
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


        private static IList<CrimeEntry> ReadCrimeData(string fileContent)
        {
            using var reader = new StringReader(fileContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            return csv.GetRecords<CrimeEntry>().ToList();
        }
    }
}
