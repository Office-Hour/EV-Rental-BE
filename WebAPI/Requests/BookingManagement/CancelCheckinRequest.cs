using Application.DTOs.BookingManagement;

namespace WebAPI.Requests.BookingManagement;

public class CancelCheckinRequest
{
    public Guid BookingId { get; set; }
    public string CancelReason { get; set; } = string.Empty;
    public string CancelCheckinCode { get; set; } = string.Empty;
    public RenterBankAccountDto? RenterBankAccount { get; set; }
}
