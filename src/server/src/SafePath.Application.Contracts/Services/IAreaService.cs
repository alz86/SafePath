using SafePath.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    /// <summary>
    /// Class representing the methods available to
    /// work with entities of type <see cref="Area"/>.
    /// </summary>
    public interface IAreaService : IApplicationService
    {
        /// <summary>
        /// Gets the list of areas that the current user
        /// is able to admin.
        /// </summary>
        Task<IList<AreaDto>> GetAdminAreas();

        /// <summary>
        /// Gets the list of security elements (police stations,
        /// hospital, bus stops, etc.) associated to the system's 
        /// default area.
        /// </summary>
        Task<IList<MapSecurityElementDto>> GetSecurityElements(Guid areaId);

        /// <summary>
        /// Gets the GeoJSON representation of an extra layer drawn over
        /// system's maps to show the different security elements mapped.
        /// </summary>
        Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON(Guid areaId);

        Task ClearSecurityInfoCache(Guid areaId);
    }
}