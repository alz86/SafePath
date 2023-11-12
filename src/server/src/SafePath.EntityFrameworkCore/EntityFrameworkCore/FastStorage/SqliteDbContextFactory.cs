using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class SqliteDbContextFactory : DbContextFactoryBase<SqliteDbContext>
    {
        protected override string ConnectionStringName => "Sqlite";

        protected override SqliteDbContext ConfigureDbContext(string connectionString)
        {
            var newConnectionString = FixDataSourcePath(connectionString);
            var builder = new DbContextOptionsBuilder<SqliteDbContext>().UseSqlite(newConnectionString);

            return new SqliteDbContext(builder.Options);
        }

        /// <summary>
        /// Fixes the connection string to point the 
        /// data file to the right folder.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <remarks>
        /// Connection string is "Data Source=<path>" and we need to ensure it actually points to the Data folder in the website
        /// </remarks>
        private static string FixDataSourcePath(string connectionString)
        {
            var sections = connectionString.Split(';');
            var idx = sections.FindIndex(p => p.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase));
            var dataSource = sections[idx];
            var parts = dataSource.Split("=");
            var fixedPath = Path.Combine(@"..\SafePath.HttpApi.Host\Data\Resources", parts[1]);
            var newDataSource = $"{parts[0]}={fixedPath}";
            sections[idx] = newDataSource;

            return string.Join(';', sections);
        }
    }
}
