using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public interface ITestDataService : IApplicationService
    {
        Task AddTestData();
    }
}
