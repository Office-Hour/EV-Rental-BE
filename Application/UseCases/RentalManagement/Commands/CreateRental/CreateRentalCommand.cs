using MediatR;

namespace Application.UseCases.RentalManagement.Commands.CreateRental;

public class CreateRentalCommand : IRequest<Guid>
{
    public Guid BookingId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
