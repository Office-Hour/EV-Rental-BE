using MediatR;

namespace Application.UseCases.RentalManagement.Commands.ReceiveVehicle;

public class ReceiveVehicleCommand : IRequest
{
    public Guid RentalId { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public Guid ReceivedByStaffId { get; set; }
}
