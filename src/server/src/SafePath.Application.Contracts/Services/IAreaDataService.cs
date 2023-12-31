﻿using SafePath.Classes;
using SafePath.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public interface IAreaDataService : IApplicationService
    {
        Task<MapElementUpdateResult> UpdatePoint(Guid areaId, PointDto point);

        Task<List<MapElementUpdateResult>> UpdatePoints(Guid areaId, IEnumerable<PointDto> points);


        /// <summary>
        /// Uploads and processes a CSV file containing crime report data.
        /// </summary>
        /// <param name="fileContent">
        /// Full content of the CSV file to be processed.
        /// </param>
        /// <remarks>
        /// The maximum size allowed for this file is 50mb.
        /// </remarks>
        Task UploadCrimeReportCSV(Guid areaId, string fileContent);

        Task UploadBulkDataCSV(Guid areaId, string fileContent);
    }
}