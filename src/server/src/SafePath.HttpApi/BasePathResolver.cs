using SafePath.Services;
using System.IO;
using System.Reflection;

namespace SafePath;

public class BasePathResolver : IBasePathResolver
{
    public BasePathResolver()
    {
        BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        const string DebugModerFolder = "\\bin\\Debug\\net7.0";
        if (BasePath.ToLowerInvariant().EndsWith(DebugModerFolder.ToLowerInvariant()))
        {
            //we are running in debug mode either from the website or the OSMParser app.
            //the base path is where the main files are
            BasePath = BasePath[..^DebugModerFolder.Length];
        }
    }

    public string BasePath { get; private set; }
}

