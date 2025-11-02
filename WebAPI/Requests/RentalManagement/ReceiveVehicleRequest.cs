namespace WebAPI.Requests.RentalManagement;

public class ReceiveVehicleRequest
{
    public Guid RentalId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public Guid ReceivedByStaffId { get; set; }
}
