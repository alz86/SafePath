using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Volo.Abp.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore;

public abstract class DbContextFactoryBase<T> : IDesignTimeDbContextFactory<T>
    where T : AbpDbContext<T>
{
    public T CreateDbContext(string[] args)
    {
        SafePathEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<T>()
            .UseSqlServer(configuration.GetConnectionString(ConnectionStringName));

        return BuildContext(builder.Options);
    }

    protected abstract string ConnectionStringName { get; }

    protected abstract T BuildContext(DbContextOptions<T> options);

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SafePath.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
