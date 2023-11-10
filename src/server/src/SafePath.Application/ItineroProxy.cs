using Itinero;
using System.Threading.Tasks;
using Itinero.Profiles;
using Itinero.Safety;
using SafePath.DTOs;
using Volo.Abp.DependencyInjection;
using System.Text.Json;
using Itinero.SafePath;
using System.Collections.Generic;
using System;
using System.Linq;
using static SafePath.ItineroProxy;
using SafePath.Services;
using SafePath.Entities.FastStorage;

namespace SafePath
{
    /// <summary>
    /// Class providing methods to use Itinero functionalities
    /// from SafePath
    /// </summary>
    /// <remarks>
    /// This class acts as a bridge between SafePath
    /// and Itinero, meant to be the single point of contact
    /// between both sections of the system.
    /// </remarks>
    public interface IItineroProxy : ISingletonDependency
    {
        /// <summary>
        /// Calculates a pedestrian route based on the
        /// supplied coordinates
        /// </summary>
        /// <returns>String with route path in Geocode Json</returns>
        string CalculateRoute(SupportedProfile profile, float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude);

        /// <summary>
        /// Gets the EdgeId Itinero associates to the
        /// supplied coordinates
        /// </summary>
        PointSearchDto GetItineroEdgeIds(float latitude, float longitude);

        /// <summary>
        /// Initializes the object, doing heavy task like
        /// reading the map DB from the disk.
        /// </summary>
        Task Init(string[] folderKeys);

        /// <summary>
        /// Indicates whether the object has been
        /// initied.
        /// </summary>
        bool Initied { get; }

        /// <summary>
        /// Gets the list of security elements (police stations,
        /// hospital, bus stops, etc.) associated to the system's 
        /// default area.
        /// </summary>
        IReadOnlyList<MapElement> SecurityElements { get; }

        /// <summary>
        /// Gets the GeoJSON representation of an extra layer drawn over
        /// system's maps to show the different security elements mapped.
        /// </summary>
        GeoJsonFeatureCollection SecurityLayerGeoJSON { get; }


        Task UpdatePoint(Guid areaId, IEnumerable<PointDto> points);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class ItineroProxy : IItineroProxy
    {
        /// <summary>
        /// Gets the list of routing profiles that SafePath currently
        /// supports
        /// </summary>
        private static readonly string[] SupportedProfiles = new[] { "pedestrian", "bicycle" };

        const int SearchDistanceInMeters = 50;

        private RouterDb? routerDb;
        private Router? router;
        private IProfileInstance[]? profiles;

        private IList<MapElement>? securityElementsList;
        private IReadOnlyList<MapElement>? securityElements;
        private GeoJsonFeatureCollection? securityLayerGeoJSON;

        private readonly IStorageProviderService storageProviderService;
        private readonly ISafetyScoreRepository safetyScoreRepository;

        public ItineroProxy(IStorageProviderService storageProviderService, ISafetyScoreRepository safetyScoreRepository)
        {
            this.storageProviderService = storageProviderService;
            this.safetyScoreRepository = safetyScoreRepository;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public IReadOnlyList<MapElement> SecurityElements
        {
            get
            {
                EnsureInited();
                return securityElements!;
            }
            private set { securityElements = value; }
        }


        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public GeoJsonFeatureCollection SecurityLayerGeoJSON
        {
            get
            {
                EnsureInited();
                return securityLayerGeoJSON!;
            }
            private set { securityLayerGeoJSON = value; }
        }

        /// <summary>
        /// Ensures that the current has been initialized.
        /// Otherwise, it fails.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the object hasn't been initialized.
        /// </exception>
        private void EnsureInited()
        {
            if (!Initied) throw new InvalidOperationException($"Tried to accesss an ItineroProxy object before it was initialized.");
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool Initied { get; private set; } = false;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task Init(string[] folderKeys)
        {
            await Task.WhenAll(new[]
            {
                Task.Run(() => InitItineroRouter(folderKeys.Append(OSMDataParsingService.ItineroDbFileName))),
                Task.Run(() => LoadMapLibreLayer(folderKeys.Append(OSMDataParsingService.MapLibreLayerFileName)))
            });
            Initied = true;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public string CalculateRoute(SupportedProfile profile, float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude)
        {
            EnsureInited();
            var runningProfile = GetItinieroProfile(profile);
            var profileArray = new[] { runningProfile };

            //source and target points calculation
            //TODO: add checking for points out of boundaries
            var sourcePoint = router.TryResolve(profileArray, sourceLatitude, sourceLongitude, SearchDistanceInMeters);
            var targetPoint = router.TryResolve(profileArray, destLatitude, destLongitude, SearchDistanceInMeters);

            //safepath custom weight handler creation
            var factorFn = router!.ProfileFactorAndSpeedCache.GetGetFactor(profiles![0]);
            var safeScoreHandler = new SafePathWeightHandler(factorFn, safetyScoreRepository);

            //route getting
            var route = router.TryCalculate(runningProfile, safeScoreHandler, sourcePoint.Value, targetPoint.Value);

            //TODO: add error handling!
            //return as geojson
            return route.Value.ToGeoJson();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public PointSearchDto GetItineroEdgeIds(float latitude, float longitude)
        {
            var point = router.TryResolve(profiles, latitude, longitude, SearchDistanceInMeters);

            var result = new PointSearchDto();
            result.Error = point.IsError;
            if (point.IsError)
            {
                result.ErrorMessaege = point.ErrorMessage;
            }
            else
            {
                result.EdgeId = point.Value.EdgeId;
                result.VertexId = point.Value.VertexId(routerDb);
            }
            return result;
        }

        /// <summary>
        /// Loads the extra layer created to display the security
        /// elements in the system maps.
        /// </summary>
        /// <param name="scoreParamsPath">Path of the file storing the information.</param>
        private async Task LoadMapLibreLayer(string[] layerPath) =>
            securityLayerGeoJSON = (await ReadSupportFile<GeoJsonFeatureCollection>(layerPath))!;

        

        /// <summary>
        /// Inits the components associated to Itinero
        /// </summary>
        /// <param name="routeDbDataPath">Path to the Itinero db
        /// file already created.</param>
        private void InitItineroRouter(string[] routeDbDataPath)
        {
            using (var stream = storageProviderService.OpenRead(routeDbDataPath))
                this.routerDb = RouterDb.Deserialize(stream);

            this.router = new Router(routerDb);

            this.profiles = SupportedProfiles.Select(routerDb.GetSupportedProfile).ToArray();
        }

        public enum PointUpateResult
        {
            Error,
            PointNotFound,
            Success,
            NotChanged
        }

        public async Task UpdatePoint(Guid areaId, IEnumerable<PointDto> points)
        {
            //TODO: this code duplicates what is on OSMDataParsingService, but everything will be
            //refactored soon, so it will be deleted.
            var results = new List<PointUpateResult>(points.Count());
            foreach (var point in points)
            {
                var type = (SecurityElementTypes)point.Type;
                var secElement = SecurityElements.FirstOrDefault(s => s.Lat == point.Coordinates.Lat && s.Lng == point.Coordinates.Lng);
                var isNew = secElement == null;
                if (isNew)
                {
                    var itineroInfo = GetItineroEdgeIds((float)point.Coordinates.Lat, (float)point.Coordinates.Lng);
                    if (itineroInfo.Error)
                    {
                        results.Add(PointUpateResult.PointNotFound);
                        continue;
                    }

                    secElement = new MapElement
                    {
                        Lng = point.Coordinates.Lng,
                        Lat = point.Coordinates.Lat,
                        Type = type,
                        EdgeId = itineroInfo.EdgeId!.Value,
                        VertexId = itineroInfo.VertexId!.Value,
                    };
                    securityElementsList!.Add(secElement);
                }
                else if (secElement!.Type == type)
                {
                    results.Add(PointUpateResult.NotChanged);
                    continue;
                }

                //if reached here, it is a new element or a change of type.
                //in any case, we have to update the rest of the supplementary
                //info (safety score and maplibre map).
                secElement.Type = type;



                results.Add(PointUpateResult.Success);
            }

            var hasChanged = results.Any(r => r == PointUpateResult.Success);
        }

        //TODO: refactor, it is quite wimp
        private IProfileInstance GetItinieroProfile(SupportedProfile profile) => profiles![((int)profile) - 1];

        //TODO: centralice with WriteSupportFile
        private async Task<T?> ReadSupportFile<T>(string[] path)
        {
            using var stream = storageProviderService.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream);

        }

        public enum SupportedProfile
        {
            Pedestrian = 1,
            Bike
        }

    }
}