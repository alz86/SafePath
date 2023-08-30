using SafePath.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SafePath.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SafePathController : AbpControllerBase
{
    protected SafePathController()
    {
        LocalizationResource = typeof(SafePathResource);
    }
}
