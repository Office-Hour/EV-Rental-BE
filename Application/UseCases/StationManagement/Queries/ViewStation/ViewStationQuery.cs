using Application.DTOs;
using Application.DTOs.StationManagement;
using MediatR;

namespace Application.UseCases.StationManagement.Queries.ViewStation;

public class ViewStationQuery : IRequest<PagedResult<StationDto>>
{
    public PagingDto Paging { get; init; } = null!;
}
