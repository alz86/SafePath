using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SafePath.Blazor;

[Dependency(ReplaceServices = true)]
public class SafePathBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "SafePath";
}
