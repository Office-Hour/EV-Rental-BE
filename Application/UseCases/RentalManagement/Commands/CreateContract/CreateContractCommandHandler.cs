using Application.CustomExceptions;
using Application.Interfaces;
using Domain.Entities.RentalManagement;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Commands.CreateContract;

public class CreateContractCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateContractCommand, Guid>
{
    public async Task<Guid> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var contractRepository = uow.Repository<Contract>();
        var contract = new Contract
        {
            RentalId = request.RentalId,
            Provider = request.Provider,
            DocumentHash = string.Empty, // Placeholder, to be updated after e-signature process
            DocumentUrl = string.Empty, // Placeholder, to be updated after e-signature process
            Status = ContractStatus.Issued,
            IssuedAt = DateTime.Now,
            AuditTrailUrl = string.Empty // Placeholder, to be updated after e-signature process
        };
        await contractRepository.AddAsync(contract, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return contract.ContractId;
    }
}
