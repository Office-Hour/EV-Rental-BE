namespace WebAPI.Requests.BookingManagement;

public class ViewVehiclesByStationRequest
{
    public Guid StationId { get; set; }
    public Guid? VehicleId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
