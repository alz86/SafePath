using AutoMapper;
using SafePath.DTOs;
using SafePath.Entities.FastStorage;
using Area = SafePath.Entities.Area;

namespace SafePath;

public class SafePathApplicationAutoMapperProfile : Profile
{
    public SafePathApplicationAutoMapperProfile()
    {
        CreateMap<Area, AreaDto>();
        CreateMap<MapElement, MapSecurityElementDto>();
    }
}
