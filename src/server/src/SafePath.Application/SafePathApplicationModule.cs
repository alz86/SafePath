using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace SafePath;

[DependsOn(
    typeof(SafePathDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(SafePathApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class SafePathApplicationModule : AbpModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        await base.ConfigureServicesAsync(context);
        
        //var route = @"C:\Code\SafePath\abp\SafePath\src\server\src\SafePath.HttpApi.Host\Resources\berlin-latest-parsed.rdb";
        var route = @"C:\Code\SafePath\abp\SafePath\src\server\src\SafePath.HttpApi.Host\Resources\berlin-latest.osm.routeDb.pdb";
        var safety = @"C:\Code\SafePath\abp\SafePath\src\server\src\SafePath.HttpApi.Host\Resources\berlin-latest.osm.safetyscore.values.json";

        var routeHandler = new ItineroProxy();
        await routeHandler.Init(route, safety);
        context.Services.AddSingleton<IItineroProxy>(routeHandler);

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SafePathApplicationModule>();
        });

    }
}
