namespace SafePath.Services
{
    /// <summary>
    /// Represents an object able to
    /// get the base path from where
    /// the app is running
    /// </summary>
    /// <remarks>
    /// This is a little bit hacky way of solving the base
    /// app folder but it turns out that is quite hard to determine
    /// it from inside Abp's module system. Thus, we prefer to insert
    /// this hack here and keep using the rest of the modules system
    /// as they should, instead of even more complex solutions.
    /// </remarks>
    public interface IBasePathResolver
    {
        string BasePath { get; }
    }

}
