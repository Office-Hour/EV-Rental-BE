using Domain.Enums;

namespace WebAPI.Requests.BookingManagement;

public class CreateBookingRequest
{
    public Guid RenterId { get; set; }
    public Guid VehicleAtStationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    // Deposit Fee Details
    public string DepositDescription { get; set; } = string.Empty;
    public decimal DepositAmount { get; set; }
    public Currency DepositCurrency { get; set; } = Currency.VND;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;
    public decimal AmountPaid { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? ProviderReference { get; set; }
}
