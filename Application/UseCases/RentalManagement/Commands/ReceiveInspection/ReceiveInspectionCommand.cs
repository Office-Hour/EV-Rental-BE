using Domain.Enums;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.ReceiveInspection;

public class ReceiveInspectionCommand : IRequest<Guid>
{
    public Guid InspectionId { get; set; }
    public Guid RentalId { get; set; }
    public InspectionType Type { get; set; } = InspectionType.Pre_Rental;
    public decimal CurrentBatteryCapacityKwh { get; set; }
    public DateTime InspectedAt { get; set; } = DateTime.UtcNow;
    public Guid InspectorStaffId { get; set; }
}
