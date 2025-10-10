using Application.DTOs;
using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.FilterVehiclesAvailable;

public class FilterVehiclesAvailableQuery : IRequest<PagedResult<VehicleDto>>
{
    public Guid StationId { get; set; }
    public Guid? VehicleId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
