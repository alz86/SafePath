using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public interface IClientDataValidator : IApplicationService
    {
        Task ValidateCrimeReportCSVFile(string fileContent);
    }
}
