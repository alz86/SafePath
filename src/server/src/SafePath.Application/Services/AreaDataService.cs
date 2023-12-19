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
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

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
        public async Task<MapElementUpdateResult> UpdatePoint(Guid areaId, PointDto point)
        {
            var result = await UpdatePoints(areaId, [point]);
            return result.First();
        }

        public Task<List<MapElementUpdateResult>> UpdatePoints(Guid areaId, IEnumerable<PointDto> points)
        {
            var dtos = points.Select(p => new MapElementUpdateDto
            {
                AreaId = areaId,
                Lat = p.Coordinates.Lat,
                Lng = p.Coordinates.Lng,
                ElementType = (SecurityElementTypes)p.Type,
                ElementId = p.MapElementId
            }).ToList();
            return UpdatePointsInternal(dtos);
        }

        public async Task<List<MapElementUpdateResult>> UpdatePointsInternal(IEnumerable<MapElementUpdateDto>? updateDtos, IEnumerable<MapElementUpdateDto>? deleteDtos = null)
        {
            bool hasUpdates = updateDtos?.Any() == true,
                hasDeletes = deleteDtos?.Any() == true;

            if (!hasUpdates && !hasDeletes) return [];

            var areaId = updateDtos?.FirstOrDefault()?.AreaId ?? deleteDtos?.FirstOrDefault()?.AreaId ?? Guid.Empty;
            var tracker = safetyScoreChangeTrackerFactory.Create();
            var results = new List<MapElementUpdateResult>(updateDtos?.Count() ?? 0 + deleteDtos?.Count() ?? 0);

            if (hasUpdates)
            {
                foreach (var dto in updateDtos!)
                {
                    MapElementUpdateResult result;
                    try
                    {
                        result = await UpdatePointInternal(dto, tracker);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                        result = UpdateResult(MapElementUpdateResult.ResultValues.Error);
                    }

                    results.Add(result);
                }
            }

            //delete points
            if (hasDeletes)
            {
                foreach (var dto in deleteDtos!)
                {
                    MapElementUpdateResult result;
                    try
                    {
                        result = DeleteMapElements(dto, tracker);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                        result = UpdateResult(MapElementUpdateResult.ResultValues.Error);
                    }

                    results.Add(result);
                }
            }

            //if there was changes, we have to recalculate safety scores and regenerate the MapLibre layer
            var hasChanges = results.Any(r => r.Result == MapElementUpdateResult.ResultValues.Success);
            if (hasChanges)
            {
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

            return results;
        }

        protected async Task<MapElementUpdateResult> UpdatePointInternal(MapElementUpdateDto dto, ISafetyScoreChangeTracker tracker)
        {
            //NOTE: currently the AreaId is not used.
            bool mustAdd = false, mustRecalculate = false, elementCreated = false;

            MapElement? existingElement = null;

            if (dto.ElementId.HasValue)
            {
                existingElement = mapElementRepository.GetById(dto.ElementId.Value, true);
                if (existingElement == null) return UpdateResult(MapElementUpdateResult.ResultValues.NotFound);

                var elementMoved = existingElement.Lat != dto.Lat || existingElement.Lng != dto.Lng;
                if (elementMoved)
                {
                    //user moved the element away. we have to check whether it is still
                    //on the same edge or a new one. if that happened, we need to update the new
                    //point coordinates, but also check for the previous score
                    //calculated using this point and update them
                    if (dto.EdgeId == null)
                    {
                        var itineroPoint = itineroProxy.GetItineroEdgeIds((float)dto.Lat, (float)dto.Lng);
                        //TODO: add error handling
                        if (itineroPoint.Error) return UpdateResult(MapElementUpdateResult.ResultValues.PointNotInMap);

                        dto.EdgeId = itineroPoint.EdgeId;
                        dto.VertexId = itineroPoint.VertexId;
                    }

                    var edgeChanged = dto.EdgeId != existingElement.EdgeId;
                    var typeChanged = existingElement.Type != dto.ElementType;
                    if (edgeChanged || typeChanged)
                    {
                        //sfety score in this element has to be re-calculated
                        tracker.Track(existingElement.SafetyScoreElements.ToArray());
                        //scoresToRecalculate.AddRange(existingElement.SafetyScoreElements.ToList());

                        if (edgeChanged) existingElement.SafetyScoreElements.Clear();
                    }

                    //updates the data (type is updated later)
                    existingElement.Lat = dto.Lat;
                    existingElement.Lng = dto.Lng;
                    existingElement.EdgeId = dto.EdgeId;
                    existingElement.VertexId = dto.VertexId;

                    mustRecalculate = edgeChanged || typeChanged;
                    mustAdd = edgeChanged;
                }
                else
                {
                    if (existingElement.Type == dto.ElementType)
                    {
                        //nothing changed.
                        return UpdateResult(MapElementUpdateResult.ResultValues.NoChanges);
                    }

                    //if reaches here, the type changed, so it has to 
                    //recalcuate the score
                    mustRecalculate = true;
                }

                existingElement.Type = dto.ElementType;
                mapElementRepository.Update(existingElement);
            }
            else
            {
                //TODO: prior asking to itinero for the coordinate info, check if it is
                //not already resolved and stored in the Sqlite db
                if (dto.EdgeId == null)
                {
                    var itineroPoint = itineroProxy.GetItineroEdgeIds((float)dto.Lat, (float)dto.Lng);
                    //TODO: add error handling
                    if (itineroPoint.Error) return UpdateResult(MapElementUpdateResult.ResultValues.PointNotInMap);

                    dto.EdgeId = itineroPoint.EdgeId;
                    dto.VertexId = itineroPoint.VertexId;
                }

                //if there wasn't an element Id, we still need to
                //check by coordinates if there is already an element in the DB. thus,
                //we list them alls and look for one with the same type.
                var elementsOnEdge = mapElementRepository.GetByEdgeId(dto.EdgeId!.Value);
                if (elementsOnEdge?.Any(e => e.Type == dto.ElementType) == true)
                    return UpdateResult(MapElementUpdateResult.ResultValues.DuplicatesElement); //element already exists

                //user is adding an element
                existingElement = new MapElement
                {
                    Lat = dto.Lat,
                    Lng = dto.Lng,
                    EdgeId = dto.EdgeId,
                    VertexId = dto.VertexId,
                    Type = dto.ElementType
                };
                mapElementRepository.Insert(existingElement);
                mustAdd = mustRecalculate = elementCreated = true;
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

            return new MapElementUpdateResult
            {
                MapElementId = (elementCreated ? existingElement.Id : null),
                Result = MapElementUpdateResult.ResultValues.Success
            };
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

            //data is valid, let's save it in the DB.
            var uploadingEntity = new CrimeDataUploading { RawData = fileContent, TenantId = CurrentTenant!.Id };
            uploadingEntity = await crimeDataUploadingRepository.InsertAsync(uploadingEntity);

            var entriesWithEdges = entries
                .Select(e => new { Entry = e, ItineroEdgeInfo = itineroProxy.GetItineroEdgeIds(e.Latitude, e.Longitude) })
                .ToList();

            var failedToResolve = entriesWithEdges.Where(e => e.ItineroEdgeInfo.Error).ToList();
            var entriesResolved = entriesWithEdges.Where(e => !e.ItineroEdgeInfo.Error).ToList();

            var dbEntries = mapElementRepository.FindCrimeDataByEdgeIds(entriesResolved.Select(e => e.ItineroEdgeInfo.EdgeId!.Value).ToList());

            //we have now three lists: those that failed to resolve, those that were resolved and
            //the list of entities in the DB. We have to concilated these last two, looking for changes
            //to apply to the crime reports, or delete them. For those entries not found in the DB list
            //we have to create them.

            MapElementUpdateDto toUpdateDto(CrimeEntry entry, PointSearchDto edgeInfo) =>
                new MapElementUpdateDto
                {
                    AreaId = areaId,
                    Lat = entry.Latitude,
                    Lng = entry.Longitude,
                    EdgeId = edgeInfo.EdgeId,
                    VertexId = edgeInfo.VertexId,
                    ElementType = GetCrimeReportElementType(entry.Severity),
                    ElementId = dbEntries.FirstOrDefault(d => d.EdgeId == edgeInfo.EdgeId!.Value)?.Id
                };

            var updateDtos = entriesResolved
                .Where(e => e.Entry.Severity != 0)
                .Select(entry => toUpdateDto(entry.Entry, entry.ItineroEdgeInfo))
                .ToList();


            var deleteDtos = entriesResolved
                .Where(e => e.Entry.Severity == 0)
                .Select(entry => toUpdateDto(entry.Entry, entry.ItineroEdgeInfo))
                .ToList();

            await UpdatePointsInternal(updateDtos, deleteDtos);

            /*
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
            */
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

        private MapElementUpdateResult DeleteMapElements(MapElementUpdateDto element, ISafetyScoreChangeTracker tracker)
        {
            MapElement? dbElement = null;

            if (element.ElementId != null)
            {
                dbElement = mapElementRepository.GetById(element.ElementId.Value, true);
            }
            else
            {
                if (element.EdgeId == null)
                {
                    var itineroPoint = itineroProxy.GetItineroEdgeIds((float)element.Lat, (float)element.Lng);
                    if (itineroPoint.Error)
                        return UpdateResult(MapElementUpdateResult.ResultValues.PointNotInMap);

                    element.EdgeId = itineroPoint.EdgeId;
                    element.VertexId = itineroPoint.VertexId;
                }

                var dbElements = mapElementRepository.GetByEdgeId(element.EdgeId!.Value, false);
                var isCrimeReport = element.ElementType >= SecurityElementTypes.CrimeReport_Severity_1 && element.ElementType <= SecurityElementTypes.CrimeReport_Severity_5;

                if (isCrimeReport)
                {

                }
                else
                {
                    dbElement = dbElements.FirstOrDefault(e => e.Type == element.ElementType);
                }
            }

            //checks if element was found in the DB
            if (dbElement == null)
                return UpdateResult(MapElementUpdateResult.ResultValues.NotFound);

            //element found. we track its safety score to ensure it is
            //recalculated and then proceed to delete it.
            tracker.Track(dbElement);
            mapElementRepository.Delete(dbElement);
            return UpdateResult(MapElementUpdateResult.ResultValues.Success);
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
                // UpdatePointInternal(areaId, null, p.Latitude, p.Longitude, GetCrimeReportElementType(p.Severity), tracker);
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

        private static MapElementUpdateResult UpdateResult(MapElementUpdateResult.ResultValues result, int? mapElementId = null)
                => new() { Result = result, MapElementId = mapElementId };
    }
}