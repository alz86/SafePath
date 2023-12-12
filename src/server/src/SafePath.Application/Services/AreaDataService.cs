using Microsoft.AspNetCore.Authorization;
using SafePath.Classes;
using SafePath.DTOs;
using SafePath.Entities;
using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
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
        private readonly IMaplibreLayerService maplibreLayerService;
        private readonly IDistributedCache<IList<MapSecurityElementDto>> mapSecurityElementsCache;
        private readonly IDistributedCache<GeoJsonFeatureCollection> maplibreLayerCache;
        private readonly IAreaService areaService;
        private readonly ISafetyScoreChangeTrackerFactory safetyScoreChangeTrackerFactory;

        public AreaDataService(IMapElementRepository mapElementRepository, IItineroProxy itineroProxy, ISafetyScoreElementRepository safetyScoreElementRepository, ISafetyScoreCalculator safetyScoreCalculator, IRepository<CrimeDataUploading, Guid> crimeDataUploadingRepository, IRepository<CrimeDataUploadingEntry, Guid> crimeDataUploadingEntryRepository, IItineroProxy proxy, IDataValidator dataValidator, IMaplibreLayerService maplibreLayerService, IDistributedCache<IList<MapSecurityElementDto>> mapSecurityElementsCache, IDistributedCache<GeoJsonFeatureCollection> maplibreLayerCache, IAreaService areaService, ISafetyScoreChangeTrackerFactory safetyScoreChangeTrackerFactory)
        {
            this.itineroProxy = itineroProxy;
            this.mapElementRepository = mapElementRepository;
            this.safetyScoreElementRepository = safetyScoreElementRepository;
            this.safetyScoreCalculator = safetyScoreCalculator;
            this.crimeDataUploadingRepository = crimeDataUploadingRepository;
            this.crimeDataUploadingEntryRepository = crimeDataUploadingEntryRepository;
            this.dataValidator = dataValidator;
            this.maplibreLayerService = maplibreLayerService;
            this.mapSecurityElementsCache = mapSecurityElementsCache;
            this.maplibreLayerCache = maplibreLayerCache;
            this.areaService = areaService;
            this.safetyScoreChangeTrackerFactory = safetyScoreChangeTrackerFactory;
        }

        // [AllowAnonymous]
        public Task UpdatePoint(Guid areaId, PointDto point) => UpdatePoints(areaId, [point]);

        public async Task UpdatePoints(Guid areaId, IEnumerable<PointDto> points)
        {
            var tracker = safetyScoreChangeTrackerFactory.Create();
            var hasChanges = false;
            foreach (var p in points)
            {
                var changed = await UpdatePointInternal(areaId, p.MapElementId, p.Coordinates.Lat, p.Coordinates.Lng, (SecurityElementTypes)p.Type, tracker);
                if (changed) hasChanges = true;
            }

            if (!hasChanges) return;

            var scoresUpdated = tracker.UpdateScores();

            //TODO: centralice with what is on OSMDataParsingService
            var areaBaseKeys = new[] { "Resources", areaId.ToString(), OSMDataParsingService.MapLibreLayerFileName };
            var elements = mapElementRepository.GetByAreaId(areaId);

            //TODO: lock procress using abp.io interlock mechanism
            await Task.WhenAll([
                safetyScoreElementRepository.SaveChangesAsync(),
                maplibreLayerService.GenerateMaplibreLayer(elements, areaBaseKeys)
            ]);

            await areaService.ClearSecurityInfoCache(areaId);
        }

        protected async Task<bool> UpdatePointInternal(Guid areaId, int? elementId, double lat, double lng, SecurityElementTypes elementType, ISafetyScoreChangeTracker tracker)
        {
            //NOTE: currently the AreaId is not used.
            bool mustAdd = false, mustRecalculate = false;

            MapElement? existingElement = null;

            if (elementId.HasValue)
            {
                existingElement = mapElementRepository.GetById(elementId.Value, true);
                if (existingElement == null) return false;

                var elementMoved = existingElement.Lat != lat || existingElement.Lng != lng;
                if (elementMoved)
                {
                    //user moved the element away. we have to check whether it is still
                    //on the same edge or a new one. if that happened, we need to update the new
                    //point coordinates, but also check for the previous score
                    //calculated using this point and update them

                    //TODO: add error handling
                    var itineroPoint = itineroProxy.GetItineroEdgeIds((float)lat, (float)lng);
                    if (itineroPoint.Error) return false;

                    var edgeChanged = itineroPoint.EdgeId != existingElement.EdgeId;
                    var typeChanged = existingElement.Type != elementType;
                    if (edgeChanged || typeChanged)
                    {
                        //sfety score in this element has to be re-calculated
                        tracker.Track(existingElement.SafetyScoreElements.ToArray());
                        //scoresToRecalculate.AddRange(existingElement.SafetyScoreElements.ToList());

                        if (edgeChanged) existingElement.SafetyScoreElements.Clear();
                    }

                    //updates the data (type is updated later)
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
                        return false;
                    }

                    //if reaches here, the type changed, so it has to 
                    //recalcuate the score
                    mustRecalculate = true;
                }

                existingElement.Type = elementType;
                mapElementRepository.Update(existingElement);
            }
            else
            {
                //TODO: prior asking to itinero for the coordinate info, check if it is
                //not already resolved and stored in the Sqlite db
                //TODO: add error handling
                var itineroPoint = itineroProxy.GetItineroEdgeIds((float)lat, (float)lng);
                if (itineroPoint.Error) return false;

                //if there wasn't an element Id, we still need to
                //check by coordinates if there is already an element in the DB. thus,
                //we list them alls and look for one with the same type.
                var elementsOnEdge = mapElementRepository.GetByEdgeId(itineroPoint.EdgeId!.Value);

                if (elementsOnEdge?.Any(e => e.Type == elementType) == true)
                    return false; //element already exists

                //user is adding an element
                existingElement = new MapElement
                {
                    Lat = lat,
                    Lng = lng,
                    EdgeId = itineroPoint!.EdgeId,
                    VertexId = itineroPoint.VertexId,
                    Type = elementType
                };
                mapElementRepository.Insert(existingElement);
                mustAdd = mustRecalculate = true;
            }

            //safety info
            SafetyScoreElement? safetyInfo = safetyScoreElementRepository.GetByEdgeId(existingElement.EdgeId!.Value, false);
            if (safetyInfo == null)
            {
                safetyInfo = new SafetyScoreElement
                {
                    EdgeId = existingElement.EdgeId!.Value
                };
                mustAdd = mustRecalculate = true;
            }

            if (mustAdd)
            {
                safetyInfo.MapElements.Add(existingElement);

                //saves the entity to the db            
                if (safetyInfo.Id == 0)
                    safetyScoreElementRepository.Insert(safetyInfo);
                else
                    safetyScoreElementRepository.Update(safetyInfo);

                //changes has to be saved in the DB, in order to have ids set and
                //be able to later recalculate scores. Also, we need to ensure that if
                //a new SafetyScoreElement was created, it is on the DB for next iterations,
                //in order for them to do not create a new entity associated to the EdgeId.
                await safetyScoreElementRepository.SaveChangesAsync();
                //TODO: refactor the code to avoid having to commit changes on every iteration
            }

            //marks the element to be recalculated
            if (mustRecalculate)
                tracker.Track(safetyInfo);

            return true;
        }

        public async Task UploadBulkDataCSV(Guid areaId, string fileContent)
        {
            //TODO: validate entries

        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task UploadCrimeReportCSV(Guid areaId, string fileContent)
        {
            await dataValidator.ValidateCrimeReportCSVFile(fileContent, out IList<CrimeEntry> entries);

            var tracker = safetyScoreChangeTrackerFactory.Create();

            //data is valid, let's save it in the DB.
            var uploadingEntity = new CrimeDataUploading { RawData = fileContent, TenantId = CurrentTenant!.Id };
            uploadingEntity = await crimeDataUploadingRepository.InsertAsync(uploadingEntity);

            //deletes reports with Severity = 0
            var entriesToDelete = entries
                .Where(e => e.Severity == 0)
                .Select(e => new CoordinatesDto { Lat = e.Latitude, Lng = e.Longitude })
                .ToList();

            if (entriesToDelete.Count > 0)
            {
                //delete is made in two steps. First we get the entities to delete,
                //track their associated SafetyScoreElements and then delete them.
                var elementsToDelete = mapElementRepository.FindCrimeDataByCoordinates(entriesToDelete)!;
                tracker.Track(elementsToDelete.ToArray());

                mapElementRepository.DeleteMany(elementsToDelete);
            }

            //updates the rest of the entities
            var updates = entries
                    .Where(e => e.Severity != 0)
                    .Select(e => UpdateCrimeEntry(e, tracker));
            var somethingChanged = entriesToDelete.Count > 0 || updates.Any(updated => updated == true);
            if (somethingChanged)
            {
                await Task.WhenAll([
                    mapElementRepository.SaveChangesAsync(),
                    areaService.ClearSecurityInfoCache(areaId)
                ]);
            }

            /*
            var dbEntries = entries.Where(e => e.Severity > 0).Select(e => new CrimeDataUploadingEntry
            {
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Severity = e.Severity,
                CrimeDataUploading = uploadingEntity
            });
            */
            /*
             * TODO: remove, no need to record which entry are we saving
            await crimeDataUploadingEntryRepository.InsertManyAsync(dbEntries);
            */

            //await UpdateSafetyDB(areaId, dbEntries);
            //await areaService.ClearSecurityInfoCache(areaId);
        }

        private bool UpdateCrimeEntry(CrimeEntry entry, ISafetyScoreChangeTracker tracker)
        {
            if (entry.Severity == 0) return false;

            var typeToSet = GetCrimeReportElementType(entry.Severity);

            //looks for the elements associated to the coordinates of this report
            var elements = mapElementRepository.GetByCoordinates(entry.Latitude, entry.Longitude);
            if (elements.Count > 0)
            {
                //we need to check if there are elements representing a crime report but with a different
                //level than the current one
                var crimeElement = elements.FirstOrDefault(e =>
                    e.Type == SecurityElementTypes.CrimeReport_Severity_1 ||
                    e.Type == SecurityElementTypes.CrimeReport_Severity_2 ||
                    e.Type == SecurityElementTypes.CrimeReport_Severity_3 ||
                    e.Type == SecurityElementTypes.CrimeReport_Severity_4 ||
                    e.Type == SecurityElementTypes.CrimeReport_Severity_5);

                if (crimeElement != null)
                {
                    if (crimeElement.Type == typeToSet) return false; //nothing to update

                    crimeElement.Type = typeToSet;
                    mapElementRepository.Update(crimeElement);
                    tracker.Track(crimeElement);
                    return true;
                }
            }

            //a new element has to be created

            var itineroInfo = itineroProxy.GetItineroEdgeIds((float)entry.Latitude, (float)entry.Longitude);
            //TODO: handle errors
            if (itineroInfo.Error) return false;

            var mapElement = new MapElement
            {
                Lat = entry.Latitude,
                Lng = entry.Longitude,
                Type = typeToSet,
                EdgeId = itineroInfo.EdgeId,
                VertexId = itineroInfo.VertexId,
                //TODO: resolve OSM Node Id
            };
            mapElementRepository.Insert(mapElement);

            //now we look for the associated safetyScore element. it may be one element associated
            //to the same edge id, or not.
            var score = safetyScoreElementRepository.GetByEdgeId(itineroInfo.EdgeId!.Value, false);
            if (score == null)
            {
                //if there is no element yet, then we need to create it. we also
                //calculate the score and save it. then, there is no need to 
                //track it to recalculate its score, as we do with the other
                score = new SafetyScoreElement
                {
                    EdgeId = itineroInfo.EdgeId!.Value,
                    Score = safetyScoreCalculator.Calculate([mapElement])
                };
                score.MapElements.Add(mapElement);
                safetyScoreElementRepository.Insert(score);
            }
            else
            {
                tracker.Track(score);
            }

            return true;
        }


        private Task UpdateSafetyDB(Guid areaId, IEnumerable<CrimeDataUploadingEntry> dbEntries)
        {
            var tracker = safetyScoreChangeTrackerFactory.Create();
            //points to save
            var pointsToSave = dbEntries.Where(d => d.Severity >= 1 && d.Severity <= 5);

            foreach (var p in pointsToSave)
            {
                UpdatePointInternal(areaId, null, p.Latitude, p.Longitude, GetCrimeReportElementType(p.Severity), tracker);
            }

            //TODO: complete
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

        private static SecurityElementTypes GetCrimeReportElementType(float severity)
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