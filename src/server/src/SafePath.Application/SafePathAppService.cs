using Microsoft.AspNetCore.Authorization;
using SafePath.Localization;
using Volo.Abp.Application.Services;

namespace SafePath;

[Authorize()]
public abstract class SafePathAppService : ApplicationService
{
    protected SafePathAppService()
    {
        LocalizationResource = typeof(SafePathResource);
    }
}
