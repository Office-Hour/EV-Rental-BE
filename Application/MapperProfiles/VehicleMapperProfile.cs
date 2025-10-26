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

        CreateMap<VehicleAtStation, VehicleDto>()
            .ForMember(d => d.VehicleId, m => m.MapFrom(s => s.VehicleId))
            .ForMember(d => d.VehicleAtStationId, m => m.MapFrom(s => s.VehicleAtStationId))
            .ForMember(d => d.StationId, m => m.MapFrom(s => s.StationId))
            .ForMember(d => d.StartTime, m => m.MapFrom(s => s.StartTime))
            .ForMember(d => d.EndTime, m => m.MapFrom(s => s.EndTime))
            .ForMember(d => d.Status, m => m.MapFrom(s => s.Status))
            .ForMember(d => d.CurrentBatteryCapacityKwh, m => m.MapFrom(s => (double?)s.CurrentBatteryCapacityKwh ?? s.Vehicle.BatteryCapacityKwh));

    }
}
