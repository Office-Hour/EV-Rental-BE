namespace WebAPI.Requests.RentalManagement;

public class CreateRentalRequest
{
    public Guid BookingId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
