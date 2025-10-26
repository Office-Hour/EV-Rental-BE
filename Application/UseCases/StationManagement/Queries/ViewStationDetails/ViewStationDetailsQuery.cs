using Application.DTOs.StationManagement;
using MediatR;

namespace Application.UseCases.StationManagement.Queries.ViewStationDetails;

public class ViewStationDetailsQuery : IRequest<StationDetailsDto>
{
    public Guid StationId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
