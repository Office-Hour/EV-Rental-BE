using Application.DTOs.StationManagement;
using AutoMapper;
using Domain.Entities.StationManagement;

namespace Application.MapperProfiles;

public class StationMapperProfile : Profile
{
    public StationMapperProfile()
    {
        CreateMap<Station, StationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.StationId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude));
    }
}
