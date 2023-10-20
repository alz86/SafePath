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
using SafetyInfo = Itinero.SafePath.SafetyInfo;
using Volo.Abp.Application.Services;
using SafePath.DTOs;
using SafePath.Classes;
using System.Reflection;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace SafePath.Services
{
    /// <summary>
    /// Class with different methods to parse an OSM file
    /// and get information relevant to the system.
    /// </summary>
    public interface IOSMDataParsingService : IApplicationService
    {
        /// <summary>
        /// Parses the supplied OSM file and extracts
        /// information relevant to the system.
        /// </summary>
        Task Parse(string filePath);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class OSMDataParsingService : ApplicationService, IOSMDataParsingService
    {
        private static IList<SecurityElementMapping>? mappings;
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
        public async Task Parse(string filePath)
        {
            //TODO: this is method is not running truly concurrently, despite
            //running tasks at the same time. This code has to be refactored
            //to using Task.Run instead of multiple tasks run with Task.WhenAll.

            if (!File.Exists(filePath))
                throw new Exception($"File not found at path: {filePath}");

            var itineroFilesNamingProvider = new ItineroFilesNamingProvider(filePath);

            //TODO: check if it can run in parallel to FindSecurityElements
            var routerDb = GetRouterDbFromOSM(filePath);

            //task to save routerDb to disk and read from there onwards
            var saveTask = SaveRouterDbCache(routerDb, itineroFilesNamingProvider.ItineroRouteFileName);

            //task to iterate through nodes and find elements to include
            //in the safety algorithm
            var findTask = FindSecurityElements(filePath);

            //completes both tasks
            await Task.WhenAll(new[] { saveTask, findTask });

            //now that we have the elements in OSM that represent security indicators,
            //we need to map them all to its associated Edge and vertex
            //values in routerDb, to later use it to map the route weight correctly
            var elements = findTask.Result;
            var itineroMapTask = MapElementsToItinero(routerDb, elements, itineroFilesNamingProvider.SafetyScoreParametersFileName);

            //in parallel, we calculate the safety score for every element
            var safetyScoreTask = CalculateSafetyScore(elements, itineroFilesNamingProvider.SafetyScoreValuesFileName);

            var createMaplibreLayerTask = CreateMapLibreDataLayer(elements, itineroFilesNamingProvider.MapLibreLayerFileName);

            await Task.WhenAll(new[] { itineroMapTask, safetyScoreTask, createMaplibreLayerTask });

            //TODO: adapt IItineroProxy to be able to read the data directly from memory objects.
        }

        /// <summary>
        /// Creates a GeoJSON to be used as extra layer
        /// in MapLibre to show the list of security elements
        /// mapped for this Area
        /// </summary>
        private static Task CreateMapLibreDataLayer(IReadOnlyList<MapSecurityElement> elements, string outputFilePath)
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

            return SaveSupportFile(outputFilePath, geoJson);
        }

        public static float GetSecurityScoreByType(SecurityElementTypes type)
        {
            float rate = 0;
            switch (type)
            {
                case SecurityElementTypes.PoliceStation:
                    rate = 2;
                    break;
                case SecurityElementTypes.BusStation:
                case SecurityElementTypes.Hospital:
                case SecurityElementTypes.RailwayStation:
                    rate = 1.5f;
                    break;
                case SecurityElementTypes.GovernmentBuilding:
                    rate = 1.4f;
                    break;
                case SecurityElementTypes.CCTV:
                    rate = 1.25f;
                    break;
                case SecurityElementTypes.Leisure:
                case SecurityElementTypes.Amenity:
                case SecurityElementTypes.EducationCenter:
                case SecurityElementTypes.HealthCenter:
                    rate = 1.2f;
                    break;
                case SecurityElementTypes.StreetLamp:
                    rate = 1.1f;
                    break;
                case SecurityElementTypes.Semaphore:
                    rate = 1.05f;
                    break;
                case SecurityElementTypes.Test_5_Points:
                    rate = 5;
                    break;
            }

            //TODO: add context variation
            return rate;
        }

        public static Task CalculateSafetyScore(IReadOnlyList<MapSecurityElement> elements, string outputFilePath)
        {
            var safetyInfo = new Dictionary<uint, SafetyInfo>(elements.Count);
            foreach (var element in elements)
            {
                var secRate = GetSecurityScoreByType(element.Type);
                //TODO: complete radiance
                /*
                if (element.Radiance.HasValue)
                {
                    foreach (var otherElement in elements)
                    {
                        var distance = CalculateDistance(
                            element.Lat, element.Long,
                            otherElement.Lat, otherElement.Long
                        );
                        if (distance <= element.Radiance.Value)
                        {
                            otherElement.SecurityRate += element.SecurityRate;
                        }
                    }
                }
                */

                //checks if there are already information for this place
                bool isNewElement = !safetyInfo.TryGetValue(element.EdgeId, out var info);
                if (isNewElement)
                {
                    info = new SafetyInfo
                    {
                        EdgeId = element.EdgeId,
                        VertexId = element.VertexId
                    };
                    safetyInfo.Add(info.EdgeId, info);
                }

                //information about this particular element
                var elementInfo = new ElementInfo
                {
                    Latitude = (float)element.Lat,
                    Longitude = (float)element.Lng,
                    Score = secRate,
                };
                info!.Elements.Add(elementInfo);

                //updates the ActiveElement based on the one with the higher score
                var currentScore = info.Elements[info.ActiveElement].Score;
                if (currentScore < secRate)
                {
                    info.ActiveElement = info.Elements.Count - 1;
                }
            }

            return SaveSupportFile(outputFilePath, safetyInfo);
        }

        private static async Task SaveSupportFile(string outputFilePath, object data)
        {
            //TODO: save using protobuf.
            using var stream = File.OpenWrite(outputFilePath);
            await JsonSerializer.SerializeAsync(stream, data);
        }

        /// <summary>
        /// Maps the supplied security elements to a Node/vertex in
        /// Itinero route DB.
        /// </summary>
        private static Task MapElementsToItinero(RouterDb routerDb, IReadOnlyCollection<MapSecurityElement> elements, string outputFilePath)
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

            //we save the result for future uses
            return SaveSupportFile(outputFilePath, elements);
        }

        private static Task SaveRouterDbCache(RouterDb routerDb, string filePath)
        {
            //information parsed in saved, to avoid the need of parsing it again
            using var outputStream = File.OpenWrite(filePath);
            routerDb.Serialize(outputStream);
            return Task.CompletedTask;
        }

        private static RouterDb GetRouterDbFromOSM(string filePath)
        {
            var routerDb = new RouterDb();

            //router db file parsing
            using var stream = File.OpenRead(filePath);
            routerDb.LoadOsmData(stream, new[] { Vehicle.Car, Vehicle.Bicycle, Vehicle.Pedestrian });

            //we try to resolve a random point just to initialize the profiles info
            var pedestrian = routerDb.GetSupportedProfile("pedestrian");
            var bike = routerDb.GetSupportedProfile("bicycle");
            var profiles = new IProfileInstance[] { pedestrian, bike };
            var router = new Router(routerDb);
            router.TryResolve(pedestrian, 0, 0, 10);

            return routerDb;
        }

        /// <summary>
        /// Iterates through OpenStreetMap file looking
        /// for elements to be included in the safety score
        /// algorythm
        /// </summary>
        private static Task<List<MapSecurityElement>> FindSecurityElements(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var source = new PBFOsmStreamSource(stream);

            var elements = new List<MapSecurityElement>(1000);
            // Process each OSM object.
            foreach (var osmGeo in source)
            {
                if (osmGeo.Type != OsmGeoType.Node) continue;

                var node = (Node)osmGeo;

                //we are currently adding some test data. this should be refactored
                var elementType = CheckForTestData(node);

                //if there wasn't any test data, and there are tags in the
                //node, we look for security elements
                if (elementType == null && node?.Tags?.Any() == true)
                {
                    elementType = GetElementType(node);
                }

                if (elementType == null) continue;

                var element = new MapSecurityElement
                {
                    Type = elementType.Value,
                    Lat = node!.Latitude!.Value,
                    Lng = node.Longitude!.Value,
                    OSMNodeId = node.Id!.Value
                };
                elements.Add(element);
            }

            return Task.FromResult(elements);
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
    }
}