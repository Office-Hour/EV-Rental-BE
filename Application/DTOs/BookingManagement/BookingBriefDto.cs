namespace Application.DTOs.BookingManagement;

public class BookingBriefDto
{
    public DateTime StartDate { get; set; } = default;
    public DateTime EndDate { get; set; } = default;
    public Guid RenterId { get; set; } = default;
    public Guid BookingId { get; set; } = default;
}