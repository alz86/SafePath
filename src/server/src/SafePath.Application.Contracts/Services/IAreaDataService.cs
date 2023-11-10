using SafePath.DTOs;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public interface IAreaDataService : IApplicationService
    {
        Task UpdatePoint(Guid areaId, ulong osmNodeId, PointDto point);
    }
}