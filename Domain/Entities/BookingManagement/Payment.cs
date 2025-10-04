using Domain.Enums;

namespace Domain.Entities.BookingManagement;

public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid FeeId { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Unknown;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid; // Default to Paid. Other statuses: Refunded
    public decimal AmountPaid { get; set; }
    public DateTime PaidAt { get; set; }
    public string? ProviderReference { get; set; }


    public virtual Fee Fee { get; set; } = null!;
}