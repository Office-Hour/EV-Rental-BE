using Application.UseCases.BookingManagement.Command.UploadKyc;
using AutoMapper;
using Domain.Entities.BookingManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
