using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Enums;
using MediatR;

namespace Application.UseCases.BookingManagement.Commands.CreateBooking;

public class CreateBookingCommandHandler(IUnitOfWork uow, IMapper mapper) : IRequestHandler<CreateBookingCommand>
{
    public async Task Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            var newBooking = mapper.Map<Booking>(request.CreateBookingDto);
            newBooking.RenterId = request.RenterId;
            newBooking.Status = BookingStatus.Pending_Verification;
            newBooking.VerificationStatus = BookingVerificationStatus.Pending;
            await uow.Repository<Booking>().AddAsync(newBooking, cancellationToken);

            var newDepositFee = mapper.Map<Fee>(request.DepositFeeDto);
            newDepositFee.BookingId = newBooking.BookingId;
            await uow.Repository<Fee>().AddAsync(newDepositFee, cancellationToken);

            var newDepositPayment = mapper.Map<Payment>(request.DepositFeeDto);
            newDepositPayment.FeeId = newDepositFee.FeeId;
            await uow.Repository<Payment>().AddAsync(newDepositPayment, cancellationToken);

            await uow.SaveChangesAsync(cancellationToken);
        }
        catch(Exception ex)
        {
            throw new InvalidTokenException(ex.Message);
        }
    }
}
