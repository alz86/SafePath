using Microsoft.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class SqliteDbContextFactory : DbContextFactoryBase<SqliteDbContext>
    {
        protected override string ConnectionStringName => "Sqlite";

        protected override SqliteDbContext BuildContext(DbContextOptions<SqliteDbContext> options) => new SqliteDbContext(options);
    }
}
