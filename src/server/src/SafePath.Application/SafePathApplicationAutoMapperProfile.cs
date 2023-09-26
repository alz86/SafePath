using AutoMapper;
using Itinero.SafePath;
using SafePath.DTOs;
using Area = SafePath.Entities.Area;

namespace SafePath;

public class SafePathApplicationAutoMapperProfile : Profile
{
    public SafePathApplicationAutoMapperProfile()
    {
        CreateMap<Area, AreaDto>();
        CreateMap<MapSecurityElement, MapSecurityElementDto>();
    }
}
