using Domain.Enums;

namespace Domain.Entities.BookingManagement;
public class Fee
{
    public Guid FeeId { get; set; }
    public Guid BookingId { get; set; }
    public FeeType Type { get; set; } = FeeType.Deposit; // Default to Deposit
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.VND; // Default to VND
    public DateTime CreatedAt { get; set; }


    public virtual Booking Booking { get; set; } = null!;
}