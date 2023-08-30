using System.Threading.Tasks;

namespace SafePath.Data;

public interface ISafePathDbSchemaMigrator
{
    Task MigrateAsync();
}
