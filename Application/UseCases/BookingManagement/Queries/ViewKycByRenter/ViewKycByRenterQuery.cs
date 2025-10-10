using Application.DTOs;
using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.ViewKycByRenter;

public class ViewKycByRenterQuery : IRequest<PagedResult<KycDto>>
{
    public Guid RenterId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
