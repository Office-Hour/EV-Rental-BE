namespace Domain.Entities.RentalManagement;

public class Report
{
    public Guid ReportId { get; set; }
    public Guid InspectionId { get; set; }
    public string? Notes { get; set; }
    public bool DamageFound { get; set; }


    public virtual Inspection Inspection { get; set; } = null!;
}