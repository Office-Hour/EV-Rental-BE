using Application.DTOs.RentalManagement;
using AutoMapper;
using Domain.Entities.RentalManagement;

namespace Application.MapperProfiles;

public class ContractMapperProfile : Profile
{
    public ContractMapperProfile()
    {
        CreateMap<CreateSignaturePayloadDto, Signature>();

        CreateMap<Contract, ContractDto>()
            .ReverseMap();

        CreateMap<Contract, ContractDetailsDto>();
    }
}
