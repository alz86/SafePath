using Microsoft.EntityFrameworkCore;
using SafePath.Entities.FastStorage;
using System.Threading.Tasks;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
            : base(options)
        {
        }

        public DbSet<SafetyScoreElement> SafetyScoreElements { get; set; }

        public DbSet<MapElement> MapElements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SafetyScoreElement>(b => b.ToTable("SafetyScoreElement", SafePathConsts.DbSchema));
            builder.Entity<MapElement>(b => b.ToTable("MapElement", SafePathConsts.DbSchema));

            //indexes
            builder.Entity<SafetyScoreElement>()
                .HasIndex(s => s.EdgeId)
                .IsUnique();

            builder.Entity<MapElement>()
                .HasIndex(m => new { m.Lat, m.Lng });
            
            builder.Entity<MapElement>()
                    .HasIndex(m => m.EdgeId);

            //many-to-many relationship
            builder.Entity<SafetyScoreElement>()
                .HasMany(s => s.MapElements)
                .WithMany(m => m.SafetyScoreElements)
                .UsingEntity<SafetyScoreElementMapElement>(
                j => j
                    .HasOne(sm => sm.MapElement)
                    .WithMany()
                    .HasForeignKey(sm => sm.MapElementId),
                j => j
                    .HasOne(sm => sm.SafetyScoreElement)
                    .WithMany()
                    .HasForeignKey(sm => sm.SafetyScoreElementId),
                j =>
                {
                    j.HasKey(t => new { t.SafetyScoreElementId, t.MapElementId });
                });
        }


        public async Task SetupDBToWorkInMemory()
        {
            const int CACHE_SIZE = 200_000; // 200MB

            await Task.WhenAll(new[]
            {
                Database.ExecuteSqlRawAsync($"PRAGMA mmap_size = {CACHE_SIZE * 1024};"),
                Database.ExecuteSqlRawAsync($"PRAGMA cache_size = -{CACHE_SIZE};"),
                
                //Database.ExecuteSqlRaw("PRAGMA journal_mode = MEMORY;");
                Database.ExecuteSqlRawAsync("PRAGMA synchronous = FULL;"), //slower, but safer
                Database.ExecuteSqlRawAsync("PRAGMA temp_store = MEMORY;")
            });
        }
    }
}
