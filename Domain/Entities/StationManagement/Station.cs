namespace Domain.Entities.StationManagement;

public class Station
{
    public Guid StationId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double? Latitude { get; set; } // in decimal degrees. Can be null if not provided
    public double? Longitude { get; set; } // in decimal degrees. Can be null if not provided

    /// <summary>
    /// Show the history of vehicles at different stations
    /// The current vehicles are the ones with EndTime = null
    /// </summary>
    public virtual ICollection<VehicleAtStation> VehicleAtStations { get; set; } = new List<VehicleAtStation>();
    /// <summary>
    /// Show the history of the staff at different stations
    /// The current staff are the ones with EndTime = null
    /// </summary>
    public virtual ICollection<StaffAtStation> StaffAtStations { get; set; } = new List<StaffAtStation>();
}