using Microsoft.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class SafePathDbContextFactory : DbContextFactoryBase<SafePathDbContext>
{
    public SafePathDbContextFactory() { }

    protected override string ConnectionStringName => "Default";

    protected override SafePathDbContext ConfigureDbContext(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<SafePathDbContext>().UseSqlServer(connectionString);

        return new SafePathDbContext(builder.Options);
    }
}
