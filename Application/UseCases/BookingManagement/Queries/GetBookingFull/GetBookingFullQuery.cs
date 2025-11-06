using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.GetBookingFull;

public class GetBookingFullQuery : IRequest<List<BookingDetailsDto>>
{
}
