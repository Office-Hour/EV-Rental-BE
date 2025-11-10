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
    }
}
