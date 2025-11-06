using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Commands.CreateBooking;

public class CreateBookingCommand : IRequest<Guid>
{
    public Guid RenterId { get; set; }
    public CreateBookingDto CreateBookingDto { get; set; } = null!;
    public DepositFeeDto DepositFeeDto { get; set; } = null!;
}
