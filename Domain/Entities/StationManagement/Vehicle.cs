using Domain.Entities.RentalManagement;

namespace Domain.Entities.StationManagement;
public class Vehicle
{
    public Guid VehicleId { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int ModelYear { get; set; }
    public double BatteryCapacityKwh { get; set; } 
    public double RangeKm { get; set; }


    /// <summary>
    /// Show the history of the vehicle at different stations
    /// The current station is the one with EndTime = null
    /// </summary>
    public virtual ICollection<VehicleAtStation> VehicleAtStations { get; set; } = new List<VehicleAtStation>();
    /// <summary>
    /// Gets or sets the collection of rentals associated with the vehicle.
    /// </summary>
    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    /// <summary>
    /// Gets or sets the collection of pricings associated with the vehicle.
    /// Current pricing is the one with EffectiveTo = null
    /// </summary>
    public virtual ICollection<Pricing> Pricings { get; set; } = new List<Pricing>();
}