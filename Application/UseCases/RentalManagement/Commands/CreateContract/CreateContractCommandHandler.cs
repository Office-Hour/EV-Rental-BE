using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.CreateContract;

public class CreateContractCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateContractCommand, Guid>
{
    public async Task<Guid> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var rentalRepository = uow.Repository<Domain.Entities.RentalManagement.Rental>();
        var rental = await rentalRepository.GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new Exception("Rental not found");
        var contractRepository = uow.Repository<Domain.Entities.RentalManagement.Contract>();
        var contract = new Domain.Entities.RentalManagement.Contract
        {
            RentalId = rental.RentalId,
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
