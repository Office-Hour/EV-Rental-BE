using Application.MapperProfiles;
using MediatR;

namespace Application.UseCases.RentalManagement.Queries.GetContractDetails;

public class GetContractDetailsQuery : IRequest<ContractDetailsDto>
{
    public Guid ContractId { get; set; }
}
