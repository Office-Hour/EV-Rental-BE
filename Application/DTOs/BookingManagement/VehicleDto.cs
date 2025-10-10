using Domain.Enums;

namespace Application.DTOs.BookingManagement;

public class VehicleDto
{
    public Guid VehicleAtStationId { get; set; } = default;
    public Guid VehicleId { get; set; } = default;
    public Guid StationId { get; set; } = default;
    public DateTime StartTime { get; set; } = default;
    public DateTime? EndTime { get; set; }
    public double CurrentBatteryCapacityKwh { get; set; } // can be updated if current status is different from the original capacity. Otherwise, it is the same as Vehicle.BatteryCapacityKwh
    public VehicleAtStationStatus Status { get; set; } = VehicleAtStationStatus.Available; // Available, Reserved, In_Use, Maintenance, Out_of_Service
}