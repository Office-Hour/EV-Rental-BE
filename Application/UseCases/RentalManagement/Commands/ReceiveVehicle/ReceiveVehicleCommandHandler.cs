using Application.Interfaces;
using Domain.Entities.RentalManagement;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Commands.ReceiveVehicle;

public class ReceiveVehicleCommandHandler(IUnitOfWork uow) : IRequestHandler<ReceiveVehicleCommand>
{
    public async Task Handle(ReceiveVehicleCommand request, CancellationToken cancellationToken)
    {
        var rental = await uow.Repository<Domain.Entities.RentalManagement.Rental>()
            .GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new KeyNotFoundException("Rental not found");
        
        var inspections = await uow.Repository<Inspection>().AsQueryable()
            .Where(i => i.RentalId == request.RentalId)
            .ToListAsync(cancellationToken);

        var contracts = await uow.Repository<Contract>().AsQueryable()
            .Where(c => c.RentalId == request.RentalId)
            .ToListAsync(cancellationToken);
        
        if (contracts.Count == 0)
        {
            throw new InvalidOperationException("No contracts found for this rental");
        }
        else if (contracts.Any(c => c.Status != ContractStatus.Signed))
        {
            throw new InvalidOperationException("All contracts must be signed before receiving the vehicle");
        }

        if (inspections.Count == 0)
        {
            throw new InvalidOperationException("No inspections found for this rental");
        }

        if (rental.Status != RentalStatus.In_Progress)
            throw new InvalidOperationException("Rental must be in progress before completion.");

        // Update rental status to Completed
        rental.Status = RentalStatus.In_Progress;
        
        await uow.Repository<Rental>()
            .UpdateAsync(rental.RentalId, rental, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
    }
}
