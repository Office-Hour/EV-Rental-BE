using Application.DTOs.RentalManagement;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.UseCases.RentalManagement.Queries.GetContractDetails;

public class GetContractDetailsQueryHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<GetContractDetailsQuery, ContractDetailsDto>
{
    public async Task<ContractDetailsDto> Handle(GetContractDetailsQuery request, CancellationToken cancellationToken)
    {
        var contract = await uow.Repository<Domain.Entities.RentalManagement.Contract>()
            .GetByIdAsync(request.ContractId, cancellationToken)
            ?? throw new KeyNotFoundException("Contract not found");
        var contractDetailsDto = mapper.Map<ContractDetailsDto>(contract);
        return contractDetailsDto;
    }
}
