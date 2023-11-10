using SafePath.Services;
using System.Reflection;

namespace SafeMap.OSMParser
{
    public class BaseFolderProvider : IBaseFolderProviderService
    {
        public string BaseFolder => Assembly.GetExecutingAssembly().Location;
    }
}