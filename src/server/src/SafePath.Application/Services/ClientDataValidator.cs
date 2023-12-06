using System.Threading.Tasks;

namespace SafePath.Services
{
    public class ClientDataValidator : IClientDataValidator
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
