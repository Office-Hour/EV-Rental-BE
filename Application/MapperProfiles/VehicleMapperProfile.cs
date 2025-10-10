using Application.DTOs.BookingManagement;
using AutoMapper;
using Domain.Entities.StationManagement;

namespace Application.MapperProfiles;

public class VehicleMapperProfile : Profile
{
    public VehicleMapperProfile()
    {
        CreateMap<Vehicle, VehicleDetailsDto>()
            .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.VehicleId))
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
            .ForMember(dest => dest.ModelYear, opt => opt.MapFrom(src => src.ModelYear))
            .ForMember(dest => dest.RangeKm, opt => opt.MapFrom(src => src.RangeKm));
    }
}
