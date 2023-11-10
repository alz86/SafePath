using System.IO;
using SafePath.Services;
using Microsoft.AspNetCore.Hosting;

namespace SafePath;

public partial class SafePathHttpApiHostModule
{
    public class AspnetCoreBaseFolderProviderService : IBaseFolderProviderService
    {
        private readonly IWebHostEnvironment hostingEnvironment;
        private string? _baseFolder;

        public AspnetCoreBaseFolderProviderService(IWebHostEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public string BaseFolder
        {
            get
            {
                _baseFolder ??= Path.Combine(hostingEnvironment.ContentRootPath, "Data");
                return _baseFolder;
            }
        }
    }
}
