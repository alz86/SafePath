using Itinero;
using System.IO;
using System.Threading.Tasks;
using Itinero.Profiles;
using Itinero.Safety;
using System.Threading;
using SafePath.DTOs;

namespace SafePath
{
    /// <summary>
    /// Class providing methods to use Itinero functionalities
    /// from SafePath
    /// </summary>
    public interface IItineroProxy
    {
        /// <summary>
        /// Calculates a pedestrian route based on the
        /// supplied coordinates
        /// </summary>
        /// <returns>String with route path in Geocode Json</returns>
        string CalculateRoute(float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude);

        PointSearchDto GetItineroEdgeIds(float latitude, float longitude);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class ItineroProxy : IItineroProxy
    {
        const int SearchDistanceInMeters = 50;

        private RouterDb routerDb;
        private Router router;
        private Profile pedestrianProfile;
        private Profile bikeProfile;

        /// <summary>
        /// Initializes the object, doing heavy task like
        /// reading the map DB from the disk.
        /// </summary>
        /// <param name="routeDbDataPath">Full path to the file
        /// with Itinero <see cref="RouterDb" /> object serialized.</param>
        /// <param name="scoreDataPath">Full path to the file with the list of
        /// safety scores associated to Itinero nodes.</param>
        public async Task Init(string routeDbDataPath, string scoreDataPath)
        {
            var tasks = new[]
            {
                Task.Run(() => InitRouter(routeDbDataPath)),
                InitSafeScoreHandler(scoreDataPath)
            };

            await Task.WhenAll(tasks);
        }

        private static async Task InitSafeScoreHandler(string scoreDataPath)
        {
            //TODO: do not make this object static anymore
            SafeScoreHandler.Instance = new SafeScoreHandler();
            await SafeScoreHandler.Instance.LoadProcessedValuesFromFile(scoreDataPath);
        }

        private void InitRouter(string routeDbDataPath)
        {
            using (var stream = File.OpenRead(routeDbDataPath))
                this.routerDb = RouterDb.Deserialize(stream);

            this.router = new Router(routerDb);

            pedestrianProfile = routerDb.GetSupportedProfile("pedestrian");
            bikeProfile = routerDb.GetSupportedProfile("bicycle");
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public string CalculateRoute(float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude)
        {
            var profiles = new IProfileInstance[] { pedestrianProfile };
            var sourcePoint = router.TryResolve(profiles, sourceLatitude, sourceLongitude, SearchDistanceInMeters);
            var targetPoint = router.TryResolve(profiles, destLatitude, destLongitude, SearchDistanceInMeters);

            var factorFn = router.ProfileFactorAndSpeedCache.GetGetFactor(profiles[0]);
            var safeScoreHandler = new SafePathWeightHandler(factorFn, SafeScoreHandler.Instance);

            var route = router.TryCalculate(bikeProfile, safeScoreHandler, sourcePoint.Value, targetPoint.Value);
            return route.Value.ToGeoJson();
        }

        public PointSearchDto GetItineroEdgeIds(float latitude, float longitude)
        {
            var profiles = new IProfileInstance[] { pedestrianProfile };
            var point = router.TryResolve(profiles, latitude, longitude, SearchDistanceInMeters);

            var result = new PointSearchDto();
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
    }
}