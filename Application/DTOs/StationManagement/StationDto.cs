namespace Application.DTOs.StationManagement;

public class StationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Capacity { get; set; }
}
