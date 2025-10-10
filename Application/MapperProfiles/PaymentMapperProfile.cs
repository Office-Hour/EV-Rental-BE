using Application.DTOs.BookingManagement;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Enums;

namespace Application.MapperProfiles;

public class PaymentMapperProfile : Profile
{
    public PaymentMapperProfile()
    {
        CreateMap<DepositFeeDto, Fee>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency));

        CreateMap<DepositFeeDto, Payment>()
            .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))
            .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
            .ForMember(dest => dest.ProviderReference, opt => opt.MapFrom(src => src.ProviderReference));
    }
}
