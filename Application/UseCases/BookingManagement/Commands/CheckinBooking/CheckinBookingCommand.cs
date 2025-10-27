using Domain.Enums;
using MediatR;

namespace Application.UseCases.BookingManagement.Commands.CheckinBooking;

public class CheckinBookingCommand : IRequest
{
    public Guid BookingId { get; set; }
    public Guid VerifiedByStaffId { get; set; }
    public BookingVerificationStatus BookingVerificationStatus { get; set; }
    public string? CancelReason { get; set; }
}
