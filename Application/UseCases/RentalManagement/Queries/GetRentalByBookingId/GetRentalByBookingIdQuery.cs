using Application.DTOs.RentalManagement;
using MediatR;

namespace Application.UseCases.RentalManagement.Queries.GetRentalByBookingId;

public class GetRentalByBookingIdQuery : IRequest<RentalDto?>
{
    public Guid BookingId { get; set; }
}
