using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Commands.UploadKyc;
using AutoMapper;
using Domain.Entities.BookingManagement;

namespace Application.MapperProfiles
{
    public class KycMapperProfile : Profile
    {
        public KycMapperProfile()
        {
            CreateMap<UploadKycCommand, Kyc>()
                .ForMember(dest => dest.KycId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => src.DocumentNumber))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Kyc, KycDto>()
                .ForMember(dest => dest.KycId, opt => opt.MapFrom(src => src.KycId))
                .ForMember(dest => dest.RenterId, opt => opt.MapFrom(src => src.RenterId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.DocumentNumber, opt => opt.MapFrom(src => src.DocumentNumber))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.SubmittedAt))
                .ForMember(dest => dest.VerifiedAt, opt => opt.MapFrom(src => src.VerifiedAt))
                .ForMember(dest => dest.VerifiedByStaffId, opt => opt.MapFrom(src => src.VerifiedByStaffId))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason));
        }
    }
}
