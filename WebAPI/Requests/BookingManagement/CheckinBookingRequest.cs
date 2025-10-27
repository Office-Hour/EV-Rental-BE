using Domain.Enums;

namespace WebAPI.Requests.BookingManagement;

public class CheckinBookingRequest
{
    public Guid BookingId { get; set; }
    public Guid VerifiedByStaffId { get; set; }
    public BookingVerificationStatus BookingVerificationStatus { get; set; }
    public string? CancelReason { get; set; }
}
