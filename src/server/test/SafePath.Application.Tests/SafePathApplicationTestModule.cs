using Volo.Abp.Modularity;

namespace SafePath;

[DependsOn(
    typeof(SafePathApplicationModule),
    typeof(SafePathDomainTestModule)
    )]
public class SafePathApplicationTestModule : AbpModule
{

}
