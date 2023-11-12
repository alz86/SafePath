using Itinero;
using Itinero.IO.Osm;
using OsmSharp.Streams;
using OsmSharp;
using Itinero.Profiles;
using Vehicle = Itinero.Osm.Vehicles.Vehicle;
using System.Text.Json;
using Node = OsmSharp.Node;
using Itinero.SafePath;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Application.Services;
using SafePath.DTOs;
using SafePath.Classes;
using System.Reflection;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using System.Net.Http;
using Area = SafePath.Entities.Area;
using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;

namespace SafePath.Services
{
    /// <summary>
    /// Class with different methods to parse an OSM file
    /// and get information relevant to the system.
    /// </summary>
    public interface IOSMDataParsingService : IApplicationService
    {
        Task ImportData(Guid areaId);

        /// <summary>
        /// Parses the supplied OSM file and extracts
        /// information relevant to the system.
        /// </summary>
        Task Parse(Guid areaId, string[] tempFilePathKeys);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    [RemoteService(false)]
    public class OSMDataParsingService : ApplicationService, IOSMDataParsingService
    {
        public const string ItineroDbFileName = "routeDb.pdb";
        public const string MapLibreLayerFileName = "maplibre.layer.json";

        private static IList<SecurityElementMapping>? mappings;

        private readonly IRepository<Area, Guid> areaRepository;
        private readonly IStorageProviderService storageProviderService;
        private readonly IAreaSetupProgressService areaSetupProgressService;
        private readonly ISafetyScoreCalculator safetyScoreCalculator;
        private readonly IMapElementRepository mapElementRepository;
        private readonly ISafetyScoreElementRepository safetyScoreElementRepository;

        public OSMDataParsingService(
            IRepository<Area, Guid> areaRepository, IStorageProviderService storageProviderService, IAreaSetupProgressService areaSetupProgressService,
            ISafetyScoreCalculator safetyScoreCalculator, IMapElementRepository mapElementRepository, ISafetyScoreElementRepository safetyScoreElementRepository)
        {
            this.areaRepository = areaRepository;
            this.storageProviderService = storageProviderService;
            this.areaSetupProgressService = areaSetupProgressService;
            this.safetyScoreCalculator = safetyScoreCalculator;
            this.mapElementRepository = mapElementRepository;
            this.safetyScoreElementRepository = safetyScoreElementRepository;
        }

        protected static IList<SecurityElementMapping> Mappings
        {
            get
            {
                if (mappings == null)
                {
                    const string MappingsFileName = "Mappings.json";
                    const string MappingsFilePath = $@"SafePath.Resources.{MappingsFileName}";

                    mappings = ReadFromResources<IList<SecurityElementMapping>>(MappingsFilePath);
                }
                return mappings;
            }
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task Parse(Guid areaId, string[] tempFilePathKeys)
        {
            //TODO: this is method is not running truly concurrently, despite
            //running tasks at the same time. This code has to be refactored
            //to using Task.Run instead of multiple tasks run with Task.WhenAll.
            if (!await storageProviderService.Exists(tempFilePathKeys))
                throw new Exception($"File not found.");


            //TODO: check if it can run in parallel to FindSecurityElements
            var routerDb = GetRouterDbFromOSM(tempFilePathKeys);

            var areaBaseKeys = new[] { "Resources", areaId.ToString() };

            //task to save routerDb to disk and read from there onwards
            var saveTask = SaveRouterDbCache(routerDb, areaId, areaBaseKeys.Append(ItineroDbFileName));

            //task to iterate through nodes and find elements to include
            //in the safety algorithm
            var findTask = FindSecurityElements(areaId, tempFilePathKeys);

            //completes both tasks
            await Task.WhenAll(new[] { saveTask, findTask });

            //now that we have the elements in OSM that represent security indicators,
            //we need to map them all to its associated Edge and vertex
            //values in routerDb, to later use it to map the route weight correctly
            var elements = findTask.Result;
            MapElementsToItinero(routerDb, elements, areaId);

            //in parallel, we calculate the safety score for every element
            var safetyScoreTask = Task.Run(() => CalculateSafetyScore(areaId, elements));

            var createMaplibreLayerTask = CreateMapLibreDataLayer(areaId, elements, areaBaseKeys.Append(MapLibreLayerFileName));

            await Task.WhenAll(new[] { safetyScoreTask, createMaplibreLayerTask });

            //everything is done. we mark the area as completed
            areaSetupProgressService.MarkStepCompleted(areaId, AreaSetupProgress.Completed);

            //TODO: adapt IItineroProxy to be able to read the data directly from memory objects.
        }

        private RouterDb GetRouterDbFromOSM(string[] keys)
        {
            var routerDb = new RouterDb();

            //router db file parsing
            using (var stream = storageProviderService.OpenRead(keys))
                routerDb.LoadOsmData(stream, new[] { Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian });

            //we try to resolve a random point just to initialize the profiles info
            var pedestrian = routerDb.GetSupportedProfile("pedestrian");
            var bike = routerDb.GetSupportedProfile("bicycle");
            var profiles = new IProfileInstance[] { pedestrian, bike };
            var router = new Router(routerDb);
            router.TryResolve(pedestrian, 0, 0, 10);

            return routerDb;
        }

        private Task SaveRouterDbCache(RouterDb routerDb, Guid areaId, string[] keys)
        {
            //information parsed in saved, to avoid the need of parsing it again
            using var outputStream = storageProviderService.OpenWrite(keys);
            routerDb.Serialize(outputStream);
            areaSetupProgressService.MarkStepCompleted(areaId, AreaSetupProgress.BuildingItineroMap);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Iterates through OpenStreetMap file looking
        /// for elements to be included in the safety score
        /// algorythm
        /// </summary>
        private Task<List<MapElement>> FindSecurityElements(Guid areaId, string[] keys)
        {
            using var stream = storageProviderService.OpenRead(keys);
            var source = new PBFOsmStreamSource(stream);

            var elements = new List<MapElement>(50000);
            // Process each OSM object.
            foreach (var osmGeo in source)
            {
                if (osmGeo.Type != OsmGeoType.Node) continue;

                var node = (Node)osmGeo;

                //we are currently adding some test data. this should be refactored
                var elementType = CheckForTestData(node);

                //if there wasn't any test data, and there are tags in the
                //node, we look for security elements
                if (elementType == null && node.Tags?.Any() == true)
                {
                    elementType = GetElementType(node);
                }

                if (elementType == null) continue;

                var element = new MapElement
                {
                    Type = elementType.Value,
                    Lat = node.Latitude!.Value,
                    Lng = node.Longitude!.Value,
                    OSMNodeId = node.Id!.Value
                };
                elements.Add(element);
            }

            areaSetupProgressService.MarkStepCompleted(areaId, AreaSetupProgress.LookingForSecurityElements);
            return Task.FromResult(elements);
        }

        /// <summary>
        /// Maps the supplied security elements to a Node/vertex in
        /// Itinero route DB.
        /// </summary>
        private void MapElementsToItinero(RouterDb routerDb, IReadOnlyCollection<MapElement> elements, Guid areaId)
        {
            var pedestrian = routerDb.GetSupportedProfile("pedestrian");
            var bike = routerDb.GetSupportedProfile("bicycle");
            var profiles = new IProfileInstance[] { pedestrian, bike };
            var router = new Router(routerDb);

            Parallel.ForEach(elements, element =>
            {
                var point = router.TryResolve(profiles, (float)element.Lat, (float)element.Lng, 50);
                if (point.IsError)
                {
                    element.ItineroMappingError = point.ErrorMessage;
                }
                else
                {
                    element.EdgeId = point.Value.EdgeId;
                    element.VertexId = point.Value.VertexId(routerDb);
                }
            });

            //task completed
            areaSetupProgressService.MarkStepCompleted(areaId, AreaSetupProgress.MappingElementsToItinero);
        }

        private async Task CalculateSafetyScore(Guid areaId, IReadOnlyList<MapElement> elements)
        {
            var safetyInfoList = new List<SafetyScoreElement>(1000);
            foreach (var element in elements)
            {
                if (element.EdgeId == null) continue;

                //checks if there are already information for this place
                var safetyInfo = safetyInfoList.FirstOrDefault(s => s.EdgeId == element.EdgeId.Value);
                if (safetyInfo == null)
                {
                    safetyInfo = new SafetyScoreElement
                    {
                        EdgeId = element.EdgeId.Value,
                    };
                    safetyInfoList.Add(safetyInfo);
                }
                safetyInfo.MapElements.Add(element);
            }

            //once we have the whole list of elements mapped, we calculate the safety score
            //for every one
            Parallel.ForEach(safetyInfoList, si => si.Score = safetyScoreCalculator.Calculate(si.MapElements));

            await Task.WhenAll(
                mapElementRepository.InsertManyAsync(elements),
                safetyScoreElementRepository.InsertManyAsync(safetyInfoList)
            );
            await safetyScoreElementRepository.SaveChangesAsync();

            areaSetupProgressService.MarkStepCompleted(areaId, AreaSetupProgress.CalculatingSecurityScore);
        }

        /// <summary>
        /// Creates a GeoJSON to be used as extra layer
        /// in MapLibre to show the list of security elements
        /// mapped for this Area
        /// </summary>
        private async Task CreateMapLibreDataLayer(Guid areaId, IReadOnlyList<MapElement> elements, string[] keys)
        {
            var geoJson = new GeoJsonFeatureCollection();

            foreach (var element in elements)
            {
                var feature = new GeoJsonFeature
                {
                    Geometry = new GeoJsonPoint
                    {
                        Coordinates = new[] { element.Lng, element.Lat }
                    },
                    Properties = new Dictionary<string, object>
                    {
                        { "OSMNodeId", element.OSMNodeId },
                        { "type", element.Type.ToString() }
                    }
                };

                geoJson.Features.Add(feature);
            }

            await SaveSupportFile(keys, geoJson);

            areaSetupProgressService.MarkStepCompleted(areaId, AreaSetupProgress.CreateMapLibreLayer);
        }

        private async Task SaveSupportFile(string[] keys, object data)
        {
            //TODO: save using protobuf.
            using var stream = storageProviderService.OpenWrite(keys);
            await JsonSerializer.SerializeAsync(stream, data);
        }

        private static SecurityElementTypes? CheckForTestData(Node node)
        {
            //for testing purposes, we set an square on Berlin where the score is 5
            //points for every node. thus, it would be a "no-go" zone, and to access
            //any point arount it you should always taking a less direct path
            if (node.Longitude >= 13.417867109181504 && node.Longitude <= 13.426879331471543 &&
                node.Latitude >= 52.50702709960917 && node.Latitude <= 52.51120626576865)
            {
                return SecurityElementTypes.Test_5_Points;
            }
            return null;
        }

        /// <summary>
        /// Checks if the supplied element represent one of
        /// the elements to be included in the safety algorythm
        /// </summary>
        private static SecurityElementTypes? GetElementType(Node node)
        {
            SecurityElementTypes? type = null;

            foreach (var mapping in Mappings)
            {
                foreach (var element in mapping.Values)
                {

                    foreach (var value in element.Values)
                    {
                        if (node.Tags.Contains(element.Key, value))
                        {
#if DEBUG
                            if (type == null) type = mapping.Element;
                            else
                            {
                                Debug.Print("Element with more than one valid mapping: {0}", JsonSerializer.Serialize(node));
                            }
#else
                            return mapping.Element;
#endif
                        }
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Reads an object serialized in JSON from a 
        /// resource file
        /// </summary>
        /// <param name="resourcePath">Full path to the resource fail</param>
        /// <exception cref="InvalidOperationException">Resource was not found in the assembly</exception>
        private static T ReadFromResources<T>(string resourcePath)
        {
            //TODO: move somewhere else where we have common functions

            string json;
            // Opens the resource stream
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Resource {resourcePath} was not found.");

                // Reads the stream
                using StreamReader reader = new(stream);
                json = reader.ReadToEnd();
            }

            // Deserializes and returns the object
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Deserialize<T>(json, options)!;
        }

        //[Authorize(TenantManagementPermissions.Tenants.Create)]
        public async Task ImportData(Guid areaId)
        {
            var area = await areaRepository.FirstOrDefaultAsync(a => a.Id == areaId);
            if (area == null) throw new AbpException("Area not found");
            else if (area.OsmFileUrl.IsNullOrWhiteSpace()) throw new AbpException("OSM file URL not set");
            else if (area.OsmDataImported) return;

            //to show progress to the user, we have fitst "dummy" step of starting download
            areaSetupProgressService.MarkStepCompleted(area.Id, AreaSetupProgress.StartingDownload);

            //the real download starts
            var tempFileName = $"{area.Id}.osm.pbf";
            var storageKeys = new[] { "Temp", "OSM", tempFileName };
            await DownloadOSMData(area.OsmFileUrl!, storageKeys);
            areaSetupProgressService.MarkStepCompleted(area.Id, AreaSetupProgress.DownloadOSMFile);

            //after the file was downloaded, the heavy task of parsing it starts
            await Parse(area.Id, storageKeys);

            //import completed. we update the entity in the DB
            area.OsmDataImported = true;
            await areaRepository.UpdateAsync(area);
        }

        private async Task DownloadOSMData(string osmFileUrl, params string[] storageKeys)
        {
            using HttpClient client = new HttpClient();

            var response = await client.GetAsync(osmFileUrl);
            if (!response.IsSuccessStatusCode)
                throw new AbpException($"Failed to download OSM file. Status: {response.StatusCode}");

            var byteContent = await response.Content.ReadAsByteArrayAsync();
            await storageProviderService.SaveContents(byteContent, storageKeys);

            /*
            //TODO: implement download in chunks, like below
            // Download in chunks
            const int chunkSize = 1024 * 1024; // e.g., 1MB chunks
            while (true)
            {
                client.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(startRange, startRange + chunkSize - 1);
                using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to download chunk. Status: {response.StatusCode}");
                    return;
                }

                byte[] buffer = await response.Content.ReadAsByteArrayAsync();
                await fileStream.WriteAsync(buffer, 0, buffer.Length);

                // If the response is incomplete, then there's more data to download
                if (response.Content.Headers.ContentRange.Length == response.Content.Headers.ContentRange.To)
                {
                    break; // download is complete
                }

                startRange += buffer.Length;
            }
            */
        }
    }
}