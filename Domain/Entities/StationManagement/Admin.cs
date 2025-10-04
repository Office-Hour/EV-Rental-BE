namespace Domain.Entities.StationManagement;

public class Admin
{
    public Guid AdminId { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public DateTime HireDate { get; set; }

    public Guid UserId { get; set; } // Foreign key to AspNetUsers table
    /// <summary>
    /// Gets or sets the collection of vehicle transfers handled by the admin.
    /// This includes both created and approved transfer cases.
    /// </summary>
    public virtual ICollection<VehicleTransfer> VehicleTransfersHandled { get; set; } = new List<VehicleTransfer>();
    /// <summary>
    /// Gets or sets the collection of staff transfers handled by the admin.
    /// This includes both created and approved transfer cases.
    /// </summary>
    public virtual ICollection<StaffTransfer> StaffTransfersHandled { get; set; } = new List<StaffTransfer>();
}
