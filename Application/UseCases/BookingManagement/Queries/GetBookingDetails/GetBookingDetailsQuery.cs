using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.GetBookingDetails;

public class GetBookingDetailsQuery : IRequest<BookingDetailsDto>
{
    public Guid BookingId { get; set; }
}
