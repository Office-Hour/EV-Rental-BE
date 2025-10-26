using Application.DTOs.BookingManagement;

namespace Application.DTOs.StationManagement;

public class StationDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int Capacity { get; set; }
    public List<VehicleDto> Vehicles { get; set; } = null!;
}