using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using SafePath.DTOs;
using SafePath.Repositories.FastStorage;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
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
        private readonly IMapper mapper;
        private readonly IRepository<Area, Guid> areaRepository;

        private readonly IDistributedCache<IList<MapSecurityElementDto>> mapSecurityElementsCache;
        private readonly IDistributedCache<GeoJsonFeatureCollection> maplibreLayerCache;

        private readonly IMapElementRepository mapElementRepository;
        private readonly IMaplibreLayerService maplibreLayerService;

        public AreaService(IMapper mapper, IRepository<Area, Guid> areaRepository, IMapElementRepository mapElementRepository, IDistributedCache<IList<MapSecurityElementDto>> mapSecurityElementsCache, IDistributedCache<GeoJsonFeatureCollection> maplibreLayerCache, IMaplibreLayerService maplibreLayerService)
        {
            this.mapper = mapper;
            this.areaRepository = areaRepository;
            this.mapElementRepository = mapElementRepository;
            this.mapSecurityElementsCache = mapSecurityElementsCache;
            this.maplibreLayerCache = maplibreLayerCache;
            this.maplibreLayerService = maplibreLayerService;
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
        public Task<IList<MapSecurityElementDto>> GetSecurityElements(Guid areaId)
        {
            var key = $"mapSecurityElements-{areaId}";
            var data = mapSecurityElementsCache.GetOrAdd(
                key,
                () => mapper.Map<IList<MapSecurityElementDto>>(mapElementRepository.GetByAreaId(Guid.Empty)),
                () => new DistributedCacheEntryOptions()); //it doesn't expire

            return Task.FromResult(data!);
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON(Guid areaId)
        {
            var key = $"mapSecurityLayer-{areaId}";
            return maplibreLayerCache.GetOrAddAsync(
                    key,
                    () =>
                    {
                        //TODO: centralice with what is on OSMDataParsingService
                        var areaBaseKeys = new[] { "Resources", areaId.ToString(), OSMDataParsingService.MapLibreLayerFileName };
                        return maplibreLayerService.GetMaplibreLayer(areaBaseKeys);
                    },
                    () => new DistributedCacheEntryOptions())!; //it doesn't expire
        }

        // [AllowAnonymous]
        public Task ClearSecurityInfoCache(Guid areaId) =>
            Task.WhenAll([
                mapSecurityElementsCache.RemoveAsync($"mapSecurityElements-{areaId}"),
                maplibreLayerCache.RemoveAsync($"mapSecurityLayer-{areaId}")
            ]);
    }
}