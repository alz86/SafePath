using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public class ClientDataValidator : SafePathAppService, IClientDataValidator
    {
        private readonly IDataValidator dataValidator;

        public ClientDataValidator(IDataValidator dataValidator)
        {
            this.dataValidator = dataValidator;
        }

        public Task ValidateCrimeReportCSVFile(string fileContent) =>
            dataValidator.ValidateCrimeReportCSVFile(fileContent, out _);
    }
}
