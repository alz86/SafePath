using SafePath.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.TenantManagement;

namespace SafePath.Services
{
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    [RemoteService(false)]
    public class TestDataService : SafePathAppService, ITestDataService
    {
        public ITenantAppService TenantAppService { get; set; }

        public IRepository<Area, Guid> AreaRepository { get; set; }

        public IGuidGenerator GuidGenerator { get; set; }

        public async Task AddTestData()
        {
            var tenants = await TenantAppService.GetListAsync(new GetTenantsInput { });
            var berlinTenant = tenants.Items.FirstOrDefault(b => b.Name == "Berlin");

            if (berlinTenant == null)
            {
                berlinTenant = await TenantAppService.CreateAsync(new TenantCreateDto
                {
                    Name = "Berlin",
                    AdminEmailAddress = "adminberlin@mailinator.com",
                    AdminPassword = "password"
                });
            }

            var berlinArea = await AreaRepository.FirstOrDefaultAsync(a => a.TenantId == berlinTenant.Id);
            if (berlinArea == null)
            {
                berlinArea = new Area(GuidGenerator.Create())
                {
                    DisplayName = "Berlin",
                    InitialLatitude = 52.520008,
                    InitialLongitude = 13.404954,
                    OsmDataImported = true,
                    OsmFileUrl = "https://download.geofabrik.de/europe/germany/berlin-latest.osm.pbf",
                };

                await AreaRepository.InsertAsync(berlinArea);
            }
        }

    }
}
