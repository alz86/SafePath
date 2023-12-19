using Microsoft.AspNetCore.Authorization;
using SafePath.Localization;
using System;
using Volo.Abp.Application.Services;

namespace SafePath;

[Authorize()]
public abstract class SafePathAppService : ApplicationService
{
    protected SafePathAppService()
    {
        LocalizationResource = typeof(SafePathResource);
    }

    protected virtual void HandleException(Exception ex)
    {
        //TODO: complete
    }
}
