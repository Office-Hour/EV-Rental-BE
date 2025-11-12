using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
using MediatR;

namespace Application.UseCases.BookingManagement.Commands.CreateBooking;

public class CreateBookingCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<CreateBookingCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var renter = await uow.Repository<Renter>()
            .GetByIdAsync(request.RenterId, cancellationToken)
            ?? throw new NotFoundException($"Renter with ID {request.RenterId} was not found.");
        var newBooking = mapper.Map<Booking>(request.CreateBookingDto);
        newBooking.BookingId = Guid.NewGuid();
        newBooking.RenterId = request.RenterId;
        newBooking.Status = BookingStatus.Pending_Verification;
        newBooking.VerificationStatus = BookingVerificationStatus.Pending;
        await uow.Repository<Booking>().AddAsync(newBooking, cancellationToken);

        var newDepositFee = mapper.Map<Fee>(request.DepositFeeDto);
        newDepositFee.FeeId = Guid.NewGuid();
        newDepositFee.BookingId = newBooking.BookingId;
        await uow.Repository<Fee>().AddAsync(newDepositFee, cancellationToken);

        var newDepositPayment = mapper.Map<Payment>(request.DepositFeeDto);
        newDepositPayment.PaymentId = Guid.NewGuid();
        newDepositPayment.FeeId = newDepositFee.FeeId;

        //update Vehicle status to Reserved
        var vehicle = await uow.Repository<VehicleAtStation>()
            .GetByIdAsync(newBooking.VehicleAtStationId, cancellationToken);

        if(vehicle.Status != VehicleAtStationStatus.Available)
        {
            throw new NotFoundException("Vehicle is not available for booking.");
        }

        vehicle.Status = VehicleAtStationStatus.Booked;

        await uow.Repository<Payment>().AddAsync(newDepositPayment, cancellationToken);
        newDepositPayment.Status = PaymentStatus.Unpaid;
        await uow.Repository<Payment>().UpdateAsync(newDepositPayment.PaymentId, newDepositPayment, cancellationToken);

        await uow.Repository<VehicleAtStation>().UpdateAsync(vehicle.VehicleId, vehicle, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return newBooking.BookingId;
    }
}
