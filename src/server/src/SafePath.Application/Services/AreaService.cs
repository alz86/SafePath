using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using SafePath.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Area = SafePath.Entities.Area;

namespace SafePath.Services
{

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    [Authorize()]
    public class AreaService : SafePathAppService, IAreaService
    {
        private readonly IItineroProxy proxy;
        private readonly IMapper mapper;
        private readonly IRepository<Area, Guid> areaRepository;
        
        private IList<MapSecurityElementDto>? securityElements;
        private GeoJsonFeatureCollection? mapLibreGeoJSON;

        public AreaService(IMapper mapper, IItineroProxy proxy, IRepository<Area, Guid> areaRepository)
        {
            this.mapper = mapper;
            this.proxy = proxy;
            this.areaRepository = areaRepository;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public async Task<IList<AreaDto>> GetAdminAreas()
        {
            var entities = (await areaRepository.GetQueryableAsync())
                                    .WhereIf(CurrentTenant != null, a => a.TenantId == CurrentTenant!.Id)
                                    .ToImmutableList();

            return mapper.Map<IList<AreaDto>>(entities);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public Task<IList<MapSecurityElementDto>> GetSecurityElements()
        {
            securityElements ??= mapper.Map<IList<MapSecurityElementDto>>(proxy.SecurityElements);
            return Task.FromResult(securityElements);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON()
        {
            mapLibreGeoJSON ??= proxy.SecurityLayerGeoJSON;
            return Task.FromResult(mapLibreGeoJSON);
        }
    }
}
