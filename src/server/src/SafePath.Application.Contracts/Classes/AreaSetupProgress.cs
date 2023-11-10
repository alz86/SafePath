namespace SafePath.Classes
{
    public enum AreaSetupProgress
    {
        NotStarted = 0,
        StartingDownload,
        DownloadOSMFile,
        BuildingItineroMap,
        LookingForSecurityElements,
        MappingElementsToItinero,
        CalculatingSecurityScore,
        CreateMapLibreLayer,
        Completed
    }
}