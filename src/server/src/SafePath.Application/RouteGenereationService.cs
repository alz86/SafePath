using Volo.Abp.Application.Services;

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
        ///  Source: 52.51140710937834, 13.415687404804045
        ///  Target: 52.505712002376185, 13.424840946638504
        /// </remarks>
        string CalculateRoute(float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude);
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class RouteGenereationService : ApplicationService, IRouteGenereationService
    {
        public IItineroProxy ItineroProxy { get; set; }

        public string CalculateRoute(float sourceLatitude, float sourceLongitude, float destLatitude, float destLongitude) =>
            ItineroProxy.CalculateRoute(sourceLatitude, sourceLongitude, destLatitude, destLongitude);
    }
}