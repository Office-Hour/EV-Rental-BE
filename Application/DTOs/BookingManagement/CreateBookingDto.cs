namespace Application.DTOs.BookingManagement;

public class CreateBookingDto
{
    public Guid VehicleAtStationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}