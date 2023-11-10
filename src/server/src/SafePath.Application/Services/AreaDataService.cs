using SafePath.DTOs;
using System;
using System.Threading.Tasks;

namespace SafePath.Services
{
    public class AreaDataService : SafePathAppService, IAreaDataService
    {
        public Task UpdatePoint(Guid areaId, ulong osmNodeId, PointDto point)
        {
            return Task.CompletedTask;
            //currently the AreaId is not used.
        }
    }
}
