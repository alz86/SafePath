using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SafePath.Data;

/* This is used if database provider does't define
 * ISafePathDbSchemaMigrator implementation.
 */
public class NullSafePathDbSchemaMigrator : ISafePathDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
