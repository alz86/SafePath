using Itinero;
using System.IO;
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
        Task Init(string basePath);

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
        IReadOnlyList<MapSecurityElement> SecurityElements { get; }

        /// <summary>
        /// Gets the GeoJSON representation of an extra layer drawn over
        /// system's maps to show the different security elements mapped.
        /// </summary>
        GeoJsonFeatureCollection SecurityLayerGeoJSON { get; }
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class ItineroProxy : IItineroProxy
    {
        const int SearchDistanceInMeters = 50;

        private RouterDb? routerDb;
        private Router? router;
        private IProfileInstance[]? profiles;

        private IReadOnlyList<MapSecurityElement>? securityElements;
        private GeoJsonFeatureCollection? securityLayerGeoJSON;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public IReadOnlyList<MapSecurityElement> SecurityElements
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
        public async Task Init(string basePath)
        {
            //TODO: make truly parallel
            var itineroFileNamingProvider = new ItineroFilesNamingProvider(basePath);
            await Task.WhenAll(new[]
            {
                InitItineroRouter(itineroFileNamingProvider.ItineroRouteFileName),
                InitSafeScoreHandler(itineroFileNamingProvider.SafetyScoreValuesFileName),
                LoadScoreParameters(itineroFileNamingProvider.SafetyScoreParametersFileName),
                LoadMapLibreLayer(itineroFileNamingProvider.MapLibreLayerFileName)
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
            var safeScoreHandler = new SafePathWeightHandler(factorFn, SafeScoreHandler.Instance);

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
        /// Loads the information used as parameters to calculate the 
        /// safety scores.
        /// </summary>
        /// <param name="scoreParamsPath">Path of the file storing the information.</param>
        private async Task LoadScoreParameters(string scoreParamsPath)
        {
            var list = await ReadSupportFile<IList<MapSecurityElement>>(scoreParamsPath);
            SecurityElements = list!.AsReadOnly();
        }

        /// <summary>
        /// Loads the extra layer created to display the security
        /// elements in the system maps.
        /// </summary>
        /// <param name="scoreParamsPath">Path of the file storing the information.</param>
        private async Task LoadMapLibreLayer(string layerPath) =>
            securityLayerGeoJSON = (await ReadSupportFile<GeoJsonFeatureCollection>(layerPath))!;

        /// <summary>
        /// Initializes the object used to access the security scores
        /// when calculating routes.
        /// </summary>
        /// <param name="scoreDataPath">Path of the file storing the safety scores</param>
        /// <remarks>
        /// This structure is inherited from the Proof-of-concept version of the
        /// system and soon will be refactored.
        /// </remarks>
        private static Task InitSafeScoreHandler(string scoreDataPath)
        {
            //TODO: refactor to do not have an static object anymore
            SafeScoreHandler.Instance = new SafeScoreHandler();
            return SafeScoreHandler.Instance.LoadProcessedValuesFromFile(scoreDataPath);
        }

        /// <summary>
        /// Inits the components associated to Itinero
        /// </summary>
        /// <param name="routeDbDataPath">Path to the Itinero db
        /// file already created.</param>
        private Task InitItineroRouter(string routeDbDataPath)
        {
            using (var stream = File.OpenRead(routeDbDataPath))
                this.routerDb = RouterDb.Deserialize(stream);

            this.router = new Router(routerDb);

            this.profiles = SafeScoreHandler.SupportedProfiles.Select(routerDb.GetSupportedProfile).ToArray();

            return Task.CompletedTask;
        }

        //TODO: refactor, it is quite wimp
        private IProfileInstance GetItinieroProfile(SupportedProfile profile) => profiles![((int)profile) - 1];

        //TODO: centralice with WriteSupportFile
        private static async Task<T?> ReadSupportFile<T>(string path)
        {
            using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream);

        }

        public enum SupportedProfile
        {
            Pedestrian = 1,
            Bike
        }

    }
}