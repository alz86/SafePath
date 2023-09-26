using Localization.Resources.AbpUi;
using Microsoft.Extensions.DependencyInjection;
using SafePath.Localization;
using SafePath.Services;
using System.IO;
using System.Reflection;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace SafePath;

[DependsOn(
    typeof(SafePathApplicationContractsModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule)
    )]
public class SafePathHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
        context.Services.AddSingleton<IBasePathResolver>(new BasePathResolver());
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<SafePathResource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }

}

public class BasePathResolver : IBasePathResolver
{
    public BasePathResolver()
    {
        BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        const string DebugModerFolder = "\\bin\\Debug\\net7.0";
        if (BasePath.ToLowerInvariant().EndsWith(DebugModerFolder.ToLowerInvariant()))
        {
            //we are running in debug mode either from the website or the OSMParser app.
            //the base path is where the main files are
            BasePath = BasePath[..^DebugModerFolder.Length];
        }
    }

    public string BasePath { get; private set; }
}

