using SafePath.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SafePath.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SafePathEntityFrameworkCoreModule),
    typeof(SafePathApplicationContractsModule)
    )]
public class SafePathDbMigratorModule : AbpModule
{
}
