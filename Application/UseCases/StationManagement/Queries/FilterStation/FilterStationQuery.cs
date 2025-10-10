using Application.DTOs;
using Application.DTOs.StationManagement;
using MediatR;

namespace Application.UseCases.StationManagement.Queries.FilterStation;

public class FilterStationQuery : IRequest<PagedResult<StationDto>>
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
