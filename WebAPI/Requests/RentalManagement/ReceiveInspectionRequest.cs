using Domain.Enums;

namespace WebAPI.Requests.RentalManagement;

public class ReceiveInspectionRequest
{
    public Guid RentalId { get; set; }
    public decimal CurrentBatteryCapacityKwh { get; set; }
    public DateTime InspectedAt { get; set; }
    public Guid InspectorStaffId { get; set; }
}
