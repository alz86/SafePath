using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Volo.Abp.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore;

public abstract class DbContextFactoryBase<T> : IDesignTimeDbContextFactory<T>
    where T : DbContext
{
    protected abstract string ConnectionStringName { get; }

    public T CreateDbContext(string[] args)
    {
        SafePathEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        return ConfigureDbContext(configuration.GetConnectionString(ConnectionStringName)!);
    }
    
    protected abstract T ConfigureDbContext(string connectionString);

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SafePath.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
