using SafePath.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SafePath;

[DependsOn(
    typeof(SafePathEntityFrameworkCoreTestModule)
    )]
public class SafePathDomainTestModule : AbpModule
{

}
