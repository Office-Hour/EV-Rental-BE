using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Enums;
using MediatR;

namespace Application.UseCases.BookingManagement.Commands.CreateBooking;

public class CreateBookingCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<CreateBookingCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
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
        await uow.Repository<Payment>().AddAsync(newDepositPayment, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return newBooking.BookingId;
    }
}
