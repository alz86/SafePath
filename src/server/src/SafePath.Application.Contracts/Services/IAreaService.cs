using SafePath.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SafePath.Services
{
    public interface IAreaService : IApplicationService
    {
        Task<IList<AreaDto>> GetAdminAreas();

        Task<IList<MapSecurityElementDto>> GetSecurityElements();
        
        Task<GeoJsonFeatureCollection> GetSecurityLayerGeoJSON();

        Task<CrimeUploadingResultDto> UploadCrimeReportCSV(string fileContent);
    }
}