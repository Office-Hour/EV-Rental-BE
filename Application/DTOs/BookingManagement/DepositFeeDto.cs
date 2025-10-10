using Domain.Enums;

namespace Application.DTOs.BookingManagement;

public class DepositFeeDto
{
    public FeeType Type { get; set; } = FeeType.Deposit; // Default to Deposit (From fee)
    public string Description { get; set; } = null!; // From fee
    public decimal Amount { get; set; } // This is the total amount to be paid (From fee)
    public Currency Currency { get; set; } = Currency.VND; // Default to VND (From fee)
    public PaymentMethod Method { get; set; } = PaymentMethod.Unknown; // Default to Unknown (From payment)
    public decimal AmountPaid { get; set; } // This should match the Amount in Fee (From payment)
    public DateTime PaidAt { get; set; } = DateTime.UtcNow; // Default to now (From payment)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to now (From fee)
    public string? ProviderReference { get; set; } // From payment
}