using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Application.MapperProfiles
{
    public class IdentityUserMapperProfile : Profile
    {
        public IdentityUserMapperProfile()
        {
            // PSEUDOCODE
            // 1. Create map from IdentityUser to UserInfoDto
            // 2. Map the following properties:
            //    Id, UserName, Email, EmailConfirmed, PhoneNumber, PhoneNumberConfirmed
            // 3. Return the mapped result

            CreateMap<IdentityUser, UserInfoDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => src.PhoneNumberConfirmed));
        }
    }
}
