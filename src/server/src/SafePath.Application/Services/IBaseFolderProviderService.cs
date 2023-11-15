namespace SafePath.Services
{
    /// <summary>
    /// Interface representing a service able
    /// to provide the base folder from where
    /// the app is running
    /// </summary>
    public interface IBaseFolderProviderService
    {
        /// <summary>
        /// Gets the full path to the
        /// foldere from where the site
        /// is running
        /// </summary>
        string BaseFolder { get; }
    }
}
