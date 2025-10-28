using Application.DTOs.RentalManagement;
using AutoMapper;
using Domain.Entities.RentalManagement;

namespace Application.MapperProfiles;

public class RentalMapperProfile : Profile
{
    public RentalMapperProfile()
    {
        CreateMap<Rental, RentalDto>()
            .ReverseMap();
    }
}
