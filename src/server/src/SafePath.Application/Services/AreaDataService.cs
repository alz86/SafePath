using SafePath.Classes;
using SafePath.DTOs;
using SafePath.Entities;
using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SafePath.Services
{
    public class AreaDataService : SafePathAppService, IAreaDataService
    {
        private readonly IMapElementRepository mapElementRepository;
        private readonly IItineroProxy itineroProxy;
        private readonly ISafetyScoreElementRepository safetyScoreElementRepository;
        private readonly ISafetyScoreCalculator safetyScoreCalculator;
        private readonly IRepository<CrimeDataUploading, Guid> crimeDataUploadingRepository;
        private readonly IRepository<CrimeDataUploadingEntry, Guid> crimeDataUploadingEntryRepository;
        private readonly IDataValidator dataValidator;

        public AreaDataService(IMapElementRepository mapElementRepository, IItineroProxy itineroProxy, ISafetyScoreElementRepository safetyScoreElementRepository, ISafetyScoreCalculator safetyScoreCalculator, IRepository<CrimeDataUploading, Guid> crimeDataUploadingRepository, IRepository<CrimeDataUploadingEntry, Guid> crimeDataUploadingEntryRepository, IItineroProxy proxy, IDataValidator dataValidator)
        {
            this.itineroProxy = itineroProxy;
            this.mapElementRepository = mapElementRepository;
            this.safetyScoreElementRepository = safetyScoreElementRepository;
            this.safetyScoreCalculator = safetyScoreCalculator;
            this.crimeDataUploadingRepository = crimeDataUploadingRepository;
            this.crimeDataUploadingEntryRepository = crimeDataUploadingEntryRepository;
            this.dataValidator = dataValidator;
        }
        public async Task UpdatePoint(Guid areaId, PointDto point)
        {
            UpdatePointInternal(areaId, point.MapElementId, (float)point.Coordinates.Lat, (float)point.Coordinates.Lng, (SecurityElementTypes)point.Type);
            await safetyScoreElementRepository.SaveChangesAsync();
        }

        public async Task UpdatePoints(Guid areaId, IEnumerable<PointDto> points)
        {
            Parallel.ForEach(points, p =>
            {
                UpdatePointInternal(areaId, p.MapElementId, (float)p.Coordinates.Lat, (float)p.Coordinates.Lng, (SecurityElementTypes)p.Type);
            });
            await safetyScoreElementRepository.SaveChangesAsync();
        }

        protected void UpdatePointInternal(Guid areaId, int? elementId, float lat, float lng, SecurityElementTypes elementType)
        {
            //currently the AreaId is not used.
            var scoresToRecalculate = new List<SafetyScoreElement>();
            bool mustAdd = false, mustRecalculate = false;

            var existingElement = elementId.HasValue ? mapElementRepository.GetById(elementId.Value, true) : null;
            if (existingElement == null)
            {
                //TODO: add error handling
                //TODO: prior asking to itinero for the coordinate info, check if it is
                //not already resolved and stored in the Sqlite db
                var itineroPoint = itineroProxy.GetItineroEdgeIds(lat, lng);
                if (itineroPoint.Error) return;

                //user is adding an element
                existingElement = new MapElement
                {
                    Lat = lat,
                    Lng = lng,
                    EdgeId = itineroPoint.EdgeId,
                    VertexId = itineroPoint.VertexId,
                    Type = elementType
                };
                mapElementRepository.Insert(existingElement);
                mustAdd = mustRecalculate = true;
            }
            else
            {
                if (existingElement.Lat != lat || existingElement.Lng != lng)
                {
                    //user moved the element away. we have to check whether it is still
                    //on the same edge or a new one. if that happened, we need to update the new
                    //point coordinates, but also check for the previous score
                    //calculated using this point and update them

                    //TODO: add error handling
                    var itineroPoint = itineroProxy.GetItineroEdgeIds(lat, lng);
                    if (itineroPoint.Error) return;

                    var edgeChanged = itineroPoint.EdgeId != existingElement.EdgeId;
                    var typeChanged = existingElement.Type != elementType;
                    if (edgeChanged || typeChanged)
                    {
                        //marks all the associated scores to be re-calculated
                        scoresToRecalculate.AddRange(existingElement.SafetyScoreElements.ToList());
                        if (edgeChanged) existingElement.SafetyScoreElements.Clear();
                    }

                    //updates the data
                    existingElement.Lat = lat;
                    existingElement.Lng = lng;
                    existingElement.EdgeId = itineroPoint.EdgeId;
                    existingElement.VertexId = itineroPoint.VertexId;

                    mustRecalculate = edgeChanged || typeChanged;
                    mustAdd = edgeChanged;
                }
                else
                {
                    if (existingElement.Type == elementType)
                    {
                        //nothing changed.
                        return;
                    }

                    //if reaches here, the type changed, so it has to 
                    //recalcuate the score
                    mustRecalculate = true;
                }


                existingElement.Type = elementType;
                mapElementRepository.Update(existingElement);
            }


            //safety info
            SafetyScoreElement? safetyInfo = safetyScoreElementRepository.GetByEdgeId(existingElement.EdgeId!.Value, true);
            if (safetyInfo == null)
            {
                safetyInfo = new SafetyScoreElement
                {
                    EdgeId = existingElement.EdgeId!.Value
                };
                //safetyInfo.MapElements.Add(existingElement);
                mustAdd = mustRecalculate = true;
            }

            if (mustAdd)
                safetyInfo.MapElements.Add(existingElement);

            if (mustRecalculate)
                safetyInfo.Score = safetyScoreCalculator.Calculate(safetyInfo.MapElements);

            if (safetyInfo.Id == 0)
                safetyScoreElementRepository.Insert(safetyInfo);
            else
                safetyScoreElementRepository.Update(safetyInfo);

            //if some other elements were also marked to be recalculated.
            //then we go through them calculating the new score
            //TODO: this list should be returned and all the elements recalculated
            //together, since chances are that the same element need to be recalculated by
            //changes on different points
            foreach (var score in scoresToRecalculate)
            {
                var newScore = safetyScoreCalculator.Calculate(score.MapElements);
                if (newScore != score.Score)
                {
                    score.Score = newScore;
                    safetyScoreElementRepository.Update(score);
                }
            }
        }



        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task UploadCrimeReportCSV(Guid areaId, string fileContent)
        {
            await dataValidator.ValidateCrimeReportCSVFile(fileContent, out IList<CrimeEntry> entries);
            
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
            await UpdateSafetyDB(areaId, dbEntries);
        }

        private Task UpdateSafetyDB(Guid areaId, IEnumerable<CrimeDataUploadingEntry> dbEntries)
        {
            //points to save
            var pointsToSave = dbEntries.Where(d => d.Severity >= 1 && d.Severity <= 5);

            Parallel.ForEach(pointsToSave, p =>
            {
                UpdatePointInternal(areaId, null, p.Latitude, p.Longitude, GetCrimeReportElementType(p.Severity));
            });

            var pointsToDelete = dbEntries.Where(d => d.Severity == 0);
            DeletePoints(areaId, dbEntries);

            return safetyScoreElementRepository.SaveChangesAsync();
        }

        private void DeletePoints(Guid areaId, IEnumerable<CrimeDataUploadingEntry> dbEntries)
        {
            //TODO: add error handling
            var safetyScoresToCheck = new List<int>(dbEntries.Count());
            foreach (var entry in dbEntries)
            {
                var point = itineroProxy.GetItineroEdgeIds(entry.Latitude, entry.Longitude);
                if (point.Error) return;

                //when deleting entities, we have to also check which 
                //SafetyScoreElement they has associated and mark them
                //to be recalculated or, if don't have any other MapElement
                //associated, to be deleted
                var elements = mapElementRepository.GetByEdgeId(point.EdgeId!.Value, true);
                if (elements?.Any() != true) return;

                var type = GetCrimeReportElementType(entry.Severity);
                var element = elements.FirstOrDefault(e => e.Type == type);
                if (element == null) return;

                safetyScoresToCheck.AddRange(element.SafetyScoreElements.Select(s => s.Id).ToList());

                //deletes te mapElement
                mapElementRepository.Delete(element);

            }

            var scores = safetyScoreElementRepository.GetByIdList(safetyScoresToCheck);

            //deletes those scores which no longer have any map element associated
            var scoresToDelete = scores.Where(s => s.MapElements.Count == 0).Select(s => s.Id);
            safetyScoreElementRepository.DeleteMany(scoresToDelete);

            //for the rest, recalculates the score
            var scoresToRecalculate = scores.Where(s => s.MapElements.Count > 0);
            foreach (var entry in scoresToRecalculate)
            {
                var current = entry.Score;
                var newScore = safetyScoreCalculator.Calculate(entry.MapElements);

                if (current != newScore)
                {
                    entry.Score = newScore;
                    safetyScoreElementRepository.Update(entry);
                }
            }
        }

        private SecurityElementTypes GetCrimeReportElementType(float severity)
        {
            return severity switch
            {
                2 => SecurityElementTypes.CrimeReport_Severity_2,
                3 => SecurityElementTypes.CrimeReport_Severity_3,
                4 => SecurityElementTypes.CrimeReport_Severity_4,
                5 => SecurityElementTypes.CrimeReport_Severity_5,
                _ => SecurityElementTypes.CrimeReport_Severity_1,
            };
        }


        
    }
}
