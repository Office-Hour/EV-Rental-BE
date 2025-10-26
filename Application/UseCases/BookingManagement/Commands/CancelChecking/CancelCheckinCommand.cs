using Application.DTOs.BookingManagement;
using MediatR;

namespace Application.UseCases.BookingManagement.Commands.CancelChecking;

/// <summary>
/// Cancel Check-in and request refund deposit
/// </summary>
public class CancelCheckinCommand : IRequest<DepositFeeDto>
{
    /// <summary>
    /// Booking ID
    /// </summary>
    public Guid BookingId { get; set; }
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Booking cancellation reason
    /// </summary>
    public string CancelReason { get; set; } = null!;
    /// <summary>
    /// CancelCheckinCode
    /// </summary>
    public string CancelCheckinCode { get; set; } = null!;
    public RenterBankAccountDto? RenterBankAccount { get; set; }
}
