using Microsoft.Extensions.DependencyInjection;
using SafePath.Services;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
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

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SafePathApplicationModule>();
        });
    }
}