using MediatR;

namespace Application.UseCases.BookingManagement.Commands.RequestCancelCheckin;

public class RequestCancelCheckinCommand : IRequest
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
}
