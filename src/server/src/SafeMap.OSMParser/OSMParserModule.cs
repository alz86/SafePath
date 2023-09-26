using SafePath;
using Volo.Abp.Modularity;

//TODO: we are currently having some problems when loading another modules. 
//seems to be related to the logging module. For now, it is commented out.
//[DependsOn(typeof(SafePathApplicationModule))]
public class OSMParserModule : AbpModule
{
}