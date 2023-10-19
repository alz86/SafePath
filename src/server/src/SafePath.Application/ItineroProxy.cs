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
    public interface IItineroProxy : ISingletonDependency
    {
        /// <summary>
        /// Calculates a pedestrian route based on the
        /// supplied coordinates
        /// </summary>
        /// <returns>String with route path in Geocode Json</returns>
        string CalculateRoute(SupportedProfile profile, float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude);

        PointSearchDto GetItineroEdgeIds(float latitude, float longitude);

        Task Init(string basePath);

        bool Initied { get; }

        IReadOnlyList<MapSecurityElement> SecurityElements { get; }
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

        public IReadOnlyList<MapSecurityElement> SecurityElements
        {
            get
            {
                EnsureInited();
                return securityElements!;
            }
            private set { securityElements = value; }
        }

        public GeoJsonFeatureCollection SecurityLayerGeoJSON
        {
            get
            {
                EnsureInited();
                return securityLayerGeoJSON!;
            }
            private set { securityLayerGeoJSON = value; }
        }

        private void EnsureInited()
        {
            if (!Initied) throw new InvalidOperationException($"Property {nameof(SecurityElements)} cannot be accessed before the object be initialized.");
        }

        public bool Initied { get; private set; } = false;

        /// <summary>
        /// Initializes the object, doing heavy task like
        /// reading the map DB from the disk.
        /// </summary>
        public async Task Init(string basePath)
        {
            //TODO: make truly parallel
            var itineroFileNamingProvider = new ItineroFilesNamingProvider(basePath);
            await Task.WhenAll(new[]
            {
                InitRouter(itineroFileNamingProvider.ItineroRouteFileName),
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


        private async Task LoadScoreParameters(string scoreParamsPath)
        {
            var list = await ReadSupportFile<IList<MapSecurityElement>>(scoreParamsPath);
            SecurityElements = list!.AsReadOnly();
        }

        private async Task LoadMapLibreLayer(string layerPath) =>
            securityLayerGeoJSON = (await ReadSupportFile<GeoJsonFeatureCollection>(layerPath))!;

        private static Task InitSafeScoreHandler(string scoreDataPath)
        {
            //TODO: refactor to do not have an static object anymore
            SafeScoreHandler.Instance = new SafeScoreHandler();
            return SafeScoreHandler.Instance.LoadProcessedValuesFromFile(scoreDataPath);
        }

        private Task InitRouter(string routeDbDataPath)
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