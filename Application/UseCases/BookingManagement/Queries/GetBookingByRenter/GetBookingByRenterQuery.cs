using Application.DTOs;
using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.GetBookingByRenter;

public class GetBookingByRenterQuery : IRequest<PagedResult<BookingDetailsDto>>
{
    public Guid RenterId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
