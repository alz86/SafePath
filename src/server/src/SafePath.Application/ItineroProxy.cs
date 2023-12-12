using Itinero;
using Itinero.Profiles;
using Itinero.Safety;
using SafePath.DTOs;
using Volo.Abp.DependencyInjection;
using Itinero.SafePath;
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
        void Init(string[] folderKeys);

        /// <summary>
        /// Indicates whether the object has been
        /// initied.
        /// </summary>
        bool Initied { get; }
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
        private static readonly string[] SupportedProfiles = ["pedestrian", "bicycle"];

        private const int SearchDistanceInMeters = 50;

        private RouterDb? routerDb;
        private Router? router;
        private IProfileInstance[]? profiles;

        private readonly IStorageProviderService storageProviderService;
        private readonly ISafetyScoreRepository safetyScoreRepository;

        public ItineroProxy(IStorageProviderService storageProviderService, ISafetyScoreRepository safetyScoreRepository)
        {
            this.storageProviderService = storageProviderService;
            this.safetyScoreRepository = safetyScoreRepository;
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
        public void Init(string[] folderKeys)
        {
            InitItineroRouter(folderKeys.Append(OSMDataParsingService.ItineroDbFileName));
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
        /// Inits the components associated to Itinero
        /// </summary>
        /// <param name="routeDbDataPath">Path to the Itinero db
        /// file already created.</param>
        private void InitItineroRouter(string[] routeDbDataPath)
        {
            using (var stream = storageProviderService.OpenRead(routeDbDataPath))
                routerDb = RouterDb.Deserialize(stream);

            router = new Router(routerDb);

            profiles = SupportedProfiles.Select(routerDb.GetSupportedProfile).ToArray();
        }

        //TODO: refactor, it is quite wimp
        private IProfileInstance GetItinieroProfile(SupportedProfile profile) => profiles![((int)profile) - 1];

        public enum SupportedProfile
        {
            Pedestrian = 1,
            Bike
        }

        public enum PointUpateResult
        {
            Error,
            PointNotFound,
            Success,
            NotChanged
        }
    }
}