using Domain.Entities.RentalManagement;
using Domain.Entities.StationManagement;

namespace Domain.Entities.BookingManagement;

public class Staff
{
    public Guid StaffId { get; set; }
    public Guid UserId { get; set; } // Foreign key to AspNetUsers table
    public string EmployeeCode { get; set; } = null!;
    public string Position { get; set; } = null!;
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Show the history of the staff at different stations
    /// The current station is the one with EndTime = null
    /// </summary>
    public virtual ICollection<StaffAtStation> StaffAtStations { get; set; } = new List<StaffAtStation>();
    public virtual ICollection<Booking> VerifiedBookings { get; set; } = new List<Booking>();
    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    /// <summary>
    /// Include PickUp and DropOff transfers
    /// Pickup: VehicleTransfer.PickedUpByStaffId
    /// DropOff: VehicleTransfer.DroppedOffByStaffId
    /// </summary>
    public virtual ICollection<VehicleTransfer> VehicleTransfers { get; set; } = new List<VehicleTransfer>();
    /// <summary>
    /// Show the history of staff transfers from one station to another
    /// </summary>
    public virtual ICollection<StaffTransfer> StaffTransfers { get; set; } = new List<StaffTransfer>();
}