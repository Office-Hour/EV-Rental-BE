using Application.CustomExceptions;
using Application.Interfaces;
using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Commands.CheckinBooking;

public class CheckinBookingCommandHandler(IUnitOfWork uow) : IRequestHandler<CheckinBookingCommand>
{
    public async Task Handle(CheckinBookingCommand request, CancellationToken cancellationToken)
    {
        var bookingRepository = uow.Repository<Booking>();
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found");

        var staff = await uow.Repository<Staff>().GetByIdAsync(request.VerifiedByStaffId, cancellationToken)
            ?? throw new NotFoundException("Staff not found");

        var depositFee = await uow.Repository<Fee>()
            .AsQueryable().FirstOrDefaultAsync(f => f.BookingId == booking.BookingId && f.Type == FeeType.Deposit, cancellationToken);

        var depositPayment = await uow.Repository<Payment>()
            .AsQueryable().FirstOrDefaultAsync(p => p.FeeId == depositFee.FeeId, cancellationToken);

        if(depositPayment == null || depositPayment.AmountPaid < depositFee.Amount || depositPayment.Status != PaymentStatus.Paid)
        {
            throw new Exception("Deposit payment is not completed");
        }

        if (booking.Status != BookingStatus.Pending_Verification)
        {
            throw new Exception("Only bookings pending verification can be checked in");
        }
        if (request.BookingVerificationStatus == BookingVerificationStatus.Approved)
        {
            // Additional business logic for approved bookings can be added here
            booking.Status = BookingStatus.Verified;
            booking.VerificationStatus = BookingVerificationStatus.Approved;
            booking.VerifiedByStaffId = request.VerifiedByStaffId;
            booking.VerifiedAt = DateTime.UtcNow;
            bookingRepository.Update(booking);
            await uow.SaveChangesAsync(cancellationToken);
        }
        else
        { 
            booking.Status = BookingStatus.Cancelled;
            booking.VerificationStatus = request.BookingVerificationStatus;
            booking.VerifiedByStaffId = request.VerifiedByStaffId;
            booking.VerifiedAt = DateTime.UtcNow;
            booking.CancelReason = request.CancelReason;
            var vehicleRepo = uow.Repository<VehicleAtStation>();

            var vehicle =  await vehicleRepo
                .GetByIdAsync(booking.VehicleAtStationId, cancellationToken);
            vehicle.Status = VehicleAtStationStatus.Available;

            var refundAmount = depositFee.Amount; // Full refund for failed verification
            depositPayment.Status = PaymentStatus.Refunded;

            vehicleRepo.Update(vehicle);
            var paymentRepo = uow.Repository<Payment>();
            paymentRepo.Update(depositPayment);
            bookingRepository.Update(booking);
            await uow.SaveChangesAsync(cancellationToken);
        }
    }
}
