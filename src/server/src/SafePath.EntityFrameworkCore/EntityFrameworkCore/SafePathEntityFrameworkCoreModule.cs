using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using SafePath.EntityFrameworkCore.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Threading.Tasks;
using Volo.Abp;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore;

[DependsOn(
    typeof(SafePathDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class SafePathEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        SafePathEfCoreEntityExtensionMappings.Configure();
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await base.OnApplicationInitializationAsync(context);

        //configures the sqlite DB to run completely in memory
        await context
            .ServiceProvider
            .GetRequiredService<SqliteDbContext>()
            .SetupDBToWorkInMemory();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        /* Remove "includeAllEntities: true" to create
         * default repositories only for aggregate roots */
        context.Services.AddAbpDbContext<SafePathDbContext>(options =>
            options.AddDefaultRepositories(includeAllEntities: true)
        );

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also SafePathMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });

        // Sqlite configuration
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("Sqlite");

        context.Services.AddDbContext<SqliteDbContext>(
            options => options.UseSqlite(connectionString));

        //service mapping
        context.Services.AddScoped(typeof(IFastStorageRepositoryBase<>), typeof(FastStorageRepositoryBase<,>));
        context.Services.AddScoped<ISafetyScoreElementRepository, SafetyScoreElementRepository>();
        context.Services.AddScoped<IMapElementRepository, MapElementRepository>();
    }
}
