using SafePath.Classes;
using SafePath.DTOs;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public interface ISystemAdminService : IApplicationService
    {
        Task<Guid> CreateArea(CreateAreaInputDto dto);

        Task<AreaSetupProgress> GetAreaSetupProgress(Guid areaId);
    }
}