using Application.DTOs.RentalManagement;
using Application.UseCases.RentalManagement.Commands.ReceiveInspection;
using AutoMapper;
using Domain.Entities.RentalManagement;

namespace Application.MapperProfiles;

public class RentalMapperProfile : Profile
{
    public RentalMapperProfile()
    {
        CreateMap<Rental, RentalDto>()
            .ReverseMap();

        CreateMap<ReceiveInspectionCommand, Inspection>();

        CreateMap<Rental, RentalDetailsDto>()
            .ForMember(dest => dest.RentalId, opt => opt.MapFrom(src => src.RentalId))
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
            .ForMember(dest => dest.VehicleId, opt => opt.MapFrom(src => src.VehicleId))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Booking, opt => opt.Ignore())
            .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
            .ForMember(dest => dest.Contracts, opt => opt.Ignore())
            .ForMember(dest => dest.Score, opt => opt.Ignore())
            .ForMember(dest => dest.Comment, opt => opt.Ignore())
            .ForMember(dest => dest.RatedAt, opt => opt.Ignore());
    }
}
