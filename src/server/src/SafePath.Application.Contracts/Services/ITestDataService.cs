using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{

    /// <summary>
    /// Special service meant to add test data to the system
    /// for development purposes.
    /// </summary>
    public interface ITestDataService : IApplicationService
    {
        Task AddTestData();
    }
}
