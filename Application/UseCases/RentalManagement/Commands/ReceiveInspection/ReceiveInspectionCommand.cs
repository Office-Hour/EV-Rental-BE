using Domain.Enums;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.ReceiveInspection;

public class ReceiveInspectionCommand : IRequest<Guid>
{
    public Guid RentalId { get; set; }
    public decimal CurrentBatteryCapacityKwh { get; set; }
    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;
    public Guid InspectorStaffId { get; set; }
}
