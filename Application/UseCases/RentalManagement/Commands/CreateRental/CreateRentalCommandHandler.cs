using Application.Interfaces;
using Domain.Entities.RentalManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.RentalManagement.Commands.CreateRental;

public class CreateRentalCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateRentalCommand, Guid>
{
    public async Task<Guid> Handle(CreateRentalCommand request, CancellationToken cancellationToken)
    {
        var booking = await uow.Repository<Domain.Entities.BookingManagement.Booking>()
            .GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new Exception("Booking not found");
        var vehicleAtStation = await uow.Repository<VehicleAtStation>().AsQueryable()
                        .FirstOrDefaultAsync(v => v.VehicleAtStationId == booking.VehicleAtStationId)
            ?? throw new Exception("Vehicle at station not found");

        if(vehicleAtStation.Status != VehicleAtStationStatus.Booked)
        {
            throw new Exception("Vehicle is not available for rental");
        }

        var rentalRepository = uow.Repository<Rental>();
        var rental = new Rental
        {
            RentalId = Guid.NewGuid(),
            BookingId = request.BookingId,
            VehicleId = vehicleAtStation.VehicleId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = RentalStatus.Reserved
        };

        booking.Status = BookingStatus.Rental_Created;

        await rentalRepository.AddAsync(rental, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return rental.RentalId;
    }
}
