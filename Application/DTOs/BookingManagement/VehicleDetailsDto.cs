namespace Application.DTOs.BookingManagement;

public class VehicleDetailsDto
{
    public Guid VehicleId { get; set; } = default; // Vehicle
    public string Make { get; set; } = null!; // Vehicle
    public string Model { get; set; } = null!; // Vehicle
    public int ModelYear { get; set; } // Vehicle
    public double RangeKm { get; set; } // Vehicle
    public Guid VehicleAtStationId { get; set; } = default; // VehicleAtStation
    public double CurrentBatteryCapacityKwh { get; set; } // VehicleAtStation
    public decimal RentalPricePerHour { get; set; } // Pricing
    public decimal? RentalPricePerDay { get; set; } // Pricing

    public IEnumerable<BookingBriefDto> UpcomingBookings { get; set; } = null!;
}