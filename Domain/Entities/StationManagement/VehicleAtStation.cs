using Domain.Entities.BookingManagement;
using Domain.Enums;

namespace Domain.Entities.StationManagement;

public class VehicleAtStation
{
    public Guid VehicleAtStationId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid StationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double CurrentBatteryCapacityKwh { get; set; } // can be updated if current status is different from the original capacity. Otherwise, it is the same as Vehicle.BatteryCapacityKwh
    public VehicleAtStationStatus Status { get; set; } = VehicleAtStationStatus.Available; // Available, Reserved, In_Use, Maintenance, Out_of_Service


    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual Station Station { get; set; } = null!;
    /// <summary>
    /// Gets or sets the collection of bookings associated with the vehicle at the station.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}