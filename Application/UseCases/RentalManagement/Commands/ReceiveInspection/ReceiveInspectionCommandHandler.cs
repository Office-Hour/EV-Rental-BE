using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Entities.RentalManagement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Commands.ReceiveInspection;

public class ReceiveInspectionCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<ReceiveInspectionCommand, Guid>
{
    public async Task<Guid> Handle(ReceiveInspectionCommand request, CancellationToken cancellationToken)
    {
        var staff = await uow.Repository<Staff>().GetByIdAsync(request.InspectorStaffId, cancellationToken)
            ?? throw new NotFoundException("Staff Id Not found");

        var rental = await uow.Repository<Rental>().GetByIdAsync(request.RentalId, cancellationToken)
            ?? throw new NotFoundException("Rental Id Not found");

        // Check if rental has a signed contract for receiving
        var contract = await uow.Repository<Contract>()
            .AsQueryable()
            .Where(c => c.RentalId == rental.RentalId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Rental contract must be created before inspection can be received.");

        var inspection = mapper.Map<Inspection>(request);
        inspection.InspectionId = Guid.NewGuid();
        inspection.InspectorStaffId = staff.StaffId;
        inspection.RentalId = rental.RentalId;

        await uow.Repository<Inspection>().AddAsync(inspection, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return inspection.InspectionId;
    }
}
