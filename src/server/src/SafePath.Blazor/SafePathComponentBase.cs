using SafePath.Localization;
using Volo.Abp.AspNetCore.Components;

namespace SafePath.Blazor;

public abstract class SafePathComponentBase : AbpComponentBase
{
    protected SafePathComponentBase()
    {
        LocalizationResource = typeof(SafePathResource);
    }
}
