using Microsoft.EntityFrameworkCore;
using SafePath.Entities.FastStorage;
using System;
using Volo.Abp.EntityFrameworkCore;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class SqliteDbContext : AbpDbContext<SqliteDbContext>
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
                    // Puedes configurar más detalles de la tabla de unión aquí si es necesario
                });

            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            Console.WriteLine("OnConfiguring for Sqlite");
            if (optionsBuilder.IsConfigured)
            {
                Console.WriteLine("Setting up DB for memory use");
                SetupDBToWorkInMemory();
            }
        }

        private void SetupDBToWorkInMemory()
        {
            const int CACHE_SIZE = 200_000; // 200MB
            Database.ExecuteSqlRaw($"PRAGMA cache_size = -{CACHE_SIZE};");
            Database.ExecuteSqlRaw($"PRAGMA mmap_size = {CACHE_SIZE * 1024};");

            //Database.ExecuteSqlRaw("PRAGMA journal_mode = MEMORY;");
            Database.ExecuteSqlRaw("PRAGMA synchronous = FULL;"); //slower, but safer
            Database.ExecuteSqlRaw("PRAGMA temp_store = MEMORY;");
        }

    }
}
