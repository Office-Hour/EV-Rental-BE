using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Queries.ViewVehicleDetails;

public class ViewVehicleDetailsQuery : IRequest<VehicleDetailsDto>
{
    public Guid VehicleId { get; set; }
}
