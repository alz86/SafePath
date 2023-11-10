using SafePath.Classes;
using SafePath.DTOs;
using SafePath.Entities;
using SafePath.Jobs.Args;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.TenantManagement;

namespace SafePath.Services
{

    public class SystemAdminService : SafePathAppService, ISystemAdminService
    {
        private readonly ITenantAppService tenantAppService;
        private readonly IRepository<Area, Guid> areaRepository;
        private readonly IGuidGenerator guidGenerator;
        private readonly IBackgroundJobManager backgroundJobManager;
        private readonly IAreaSetupProgressService areaSetupProgressService;

        public SystemAdminService(IRepository<Area, Guid> areaRepository, ITenantAppService tenantAppService, IGuidGenerator guidGenerator, IBackgroundJobManager backgroundJobManager, IAreaSetupProgressService areaSetupProgressService)
        {
            this.areaRepository = areaRepository;
            this.tenantAppService = tenantAppService;
            this.guidGenerator = guidGenerator;
            this.backgroundJobManager = backgroundJobManager;
            this.areaSetupProgressService = areaSetupProgressService;
        }

        // [Authorize(TenantManagementPermissions.Tenants.Create)]
        public async Task<Guid> CreateArea(CreateAreaInputDto dto)
        {
            var tenants = await tenantAppService.GetListAsync(new GetTenantsInput { });
            var existsTenant = tenants.Items.Any(t => t.Name == dto.Name);
            if (existsTenant)
                throw new UserFriendlyException("There is already an Area with the supplied name.");

            //for every area we creante a new tenant, to leverage the multi-tenancy feature of abp
            var newTenant = await tenantAppService.CreateAsync(new TenantCreateDto
            {
                Name = dto.Name,
                //TODO: complete. currently we simply use the same admin email and password for all tenants
                AdminEmailAddress = CurrentUser.Email,
                AdminPassword = "1q2w3E*"
            });

            //creates the Area
            var newArea = new Area(guidGenerator.Create())
            {
                DisplayName = dto.Name,
                InitialLatitude = dto.Latitude,
                InitialLongitude = dto.Longitude,
                OsmFileUrl = dto.OSMFileUrl,

                OsmDataImported = false,
                TenantId = newTenant.Id
            };
            var area = await areaRepository.InsertAsync(newArea);

            //starts the task to import the OSM data
            var args = new AreaParsingBackgroundJobArgs { AreaId = area.Id };
            await backgroundJobManager.EnqueueAsync(args, delay: TimeSpan.Zero);
            
            //ends
            return area.Id;
        }

        public Task<AreaSetupProgress> GetAreaSetupProgress(Guid areaId) =>
            Task.FromResult(areaSetupProgressService.GetProgress(areaId));
    }
}
