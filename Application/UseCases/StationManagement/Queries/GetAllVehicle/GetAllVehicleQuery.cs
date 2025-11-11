using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.StationManagement.Queries.GetAllVehicle;

public class GetAllVehicleQuery : IRequest<IEnumerable<VehicleDetailsDto>>
{
}
