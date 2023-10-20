using System.IO;

namespace SafePath.Services
{
    /// <summary>
    /// Helper class that provides the names
    /// to use to access different files with
    /// information associated to Itinero or
    /// the safety score.
    /// </summary>
    public class ItineroFilesNamingProvider
    {
        private readonly string baseFileName;
        private readonly string basePath;

        public ItineroFilesNamingProvider(string osmFilePath)
        {
            basePath = Path.GetDirectoryName(osmFilePath) ?? "";
            baseFileName = Path.GetFileNameWithoutExtension(osmFilePath);
        }

        public string ItineroRouteFileName => Path.Combine(basePath, baseFileName + ".routeDb.pdb");

        public string SafetyScoreParametersFileName => Path.Combine(basePath, baseFileName + ".safetyscore.parameters.json");

        public string SafetyScoreValuesFileName => Path.Combine(basePath, baseFileName + ".safetyscore.values.json");

        public string MapLibreLayerFileName => Path.Combine(basePath, baseFileName + ".maplibre.layer.json");

    }
}
