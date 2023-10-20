using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using static SafePath.ItineroProxy;

namespace SafePath
{
    /// <summary>
    /// Service with methods related to the
    /// generation of safe routes.
    /// </summary>
    public interface IRouteGenereationService : IApplicationService
    {
        /// <summary>
        /// Calculates a pedestrian route based on the
        /// supplied coordinates
        /// </summary>
        /// <returns>String with route path in Geocode Json</returns>
        /// <remarks>
        /// These are the coordinates suggested for testing:
        /// Profile: Pedestrian (1)
        ///  Source: 52.51140710937834, 13.415687404804045
        ///  Target: 52.505712002376185, 13.424840946638504
        /// </remarks>
        Task<string> CalculateRoute(SupportedProfile profile, float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class RouteGenereationService : ApplicationService, IRouteGenereationService
    {
        private readonly IItineroProxy itineroProxy;
        public RouteGenereationService(IItineroProxy itineroProxy)
        {
            this.itineroProxy = itineroProxy;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public Task<string> CalculateRoute(SupportedProfile profile, float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude) =>
            Task.FromResult(itineroProxy.CalculateRoute(profile, sourceLatitude, sourceLongitude, destLatitude, destLongitude));
    }
}