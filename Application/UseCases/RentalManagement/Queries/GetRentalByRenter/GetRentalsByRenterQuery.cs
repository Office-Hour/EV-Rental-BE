using Application.DTOs;
using Application.DTOs.RentalManagement;
using MediatR;

namespace Application.UseCases.RentalManagement.Queries.GetRentalByRenter;

public class GetRentalsByRenterQuery : IRequest<PagedResult<RentalDto>>
{
    public Guid RenterId { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
