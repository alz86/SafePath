using SafePath.Localization;
using Volo.Abp.Application.Services;

namespace SafePath;

/* Inherit your application services from this class.
 */
public abstract class SafePathAppService : ApplicationService
{
    protected SafePathAppService()
    {
        LocalizationResource = typeof(SafePathResource);
    }
}
