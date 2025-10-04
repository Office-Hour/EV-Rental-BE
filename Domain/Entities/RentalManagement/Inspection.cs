using Domain.Entities.BookingManagement;
using Domain.Enums;

namespace Domain.Entities.RentalManagement;

public class Inspection
{
    public Guid InspectionId { get; set; }
    public Guid RentalId { get; set; }
    public InspectionType Type { get; set; } = InspectionType.Pre_Rental;
    public decimal CurrentBatteryCapacityKwh { get; set; }
    public DateTime InspectedAt { get; set; }
    public Guid InspectorStaffId { get; set; }


    public virtual Rental Rental { get; set; } = null!;
    public virtual Staff InspectorStaff { get; set; } = null!;
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}