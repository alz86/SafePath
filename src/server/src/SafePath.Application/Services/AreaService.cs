using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using SafePath.DTOs;
using SafePath.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SafePath.Services
{
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

        public async Task<IList<AreaDto>> GetAdminAreas()
        {
            var entities = (await areaRepository.GetQueryableAsync())
                                    .WhereIf(CurrentTenant != null, a => a.TenantId == CurrentTenant!.Id)
                                    .ToImmutableList();

            return mapper.Map<IList<AreaDto>>(entities);
        }

        public Task<IList<MapSecurityElementDto>> GetSecurityElements()
        {
            securityElements ??= mapper.Map<IList<MapSecurityElementDto>>(proxy.SecurityElements);
            return Task.FromResult(securityElements);
        }

        public Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON()
        {
            mapLibreGeoJSON ??= proxy.SecurityLayerGeoJSON;
            return Task.FromResult(mapLibreGeoJSON);
        }
    }
}
