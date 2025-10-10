using Application.DTOs.BookingManagement;
using AutoMapper;
using Domain.Entities.BookingManagement;

namespace Application.MapperProfiles;

public class BookingMapperProfile : Profile
{
    public BookingMapperProfile()
    {
        CreateMap<Booking, BookingBriefDto>()
            .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
            .ForMember(dest => dest.RenterId, opt => opt.MapFrom(src => src.RenterId))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndTime));

        CreateMap<CreateBookingDto, Booking>()
            .ForMember(dest => dest.VehicleAtStationId, opt => opt.MapFrom(src => src.VehicleAtStationId))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
            .ForMember(dest => dest.BookingCreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Enums.BookingStatus.Pending_Verification))
            .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => Domain.Enums.BookingVerificationStatus.Pending));

        CreateMap<Booking, BookingDetailsDto>()
            .ForAllMembers(opt => opt.Ignore()); // prevent accidental writes

    }
}
