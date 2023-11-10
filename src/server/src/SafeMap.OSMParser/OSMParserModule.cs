using Microsoft.Extensions.DependencyInjection;
using SafeMap.OSMParser;
using SafePath;
using SafePath.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

//TODO: we are currently having some problems when loading another modules. 
//seems to be related to the logging module. For now, it is commented out.
[DependsOn(typeof(SafePathApplicationModule), typeof(AbpBackgroundJobsModule))]
public class OSMParserModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        base.ConfigureServices(context);

        context.Services.AddScoped<IBaseFolderProviderService, BaseFolderProvider>();
    }
}
