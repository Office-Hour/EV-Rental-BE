using Application.Interfaces;
using Domain.Entities.BookingManagement;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services;

/// <summary>
/// Simulates VNPay payment gateway for demo/sandbox purposes
/// Updates payment status in real database
/// </summary>
public class PaymentSimulationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentSimulationService> _logger;

    public PaymentSimulationService(
        IUnitOfWork unitOfWork,
        ILogger<PaymentSimulationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Simulate VNPay payment URL generation
    /// </summary>
    public async Task<SimulatedPaymentResponse> CreatePaymentUrlAsync(
        Guid bookingId,
        decimal amount,
        string description,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("[PAYMENT SIM] Creating payment URL for booking {BookingId}, amount {Amount}",
                bookingId, amount);

            // Validate booking exists
            var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId, ct);
            if (booking == null)
            {
                throw new InvalidOperationException($"Booking {bookingId} not found");
            }

            // Get fee and payment
            var fee = await _unitOfWork.Repository<Fee>().AsQueryable()
                .FirstOrDefaultAsync(f => f.BookingId == bookingId && f.Type == FeeType.Deposit, ct);

            if (fee == null)
            {
                throw new InvalidOperationException($"Deposit fee not found for booking {bookingId}");
            }

            var payment = await _unitOfWork.Repository<Payment>().AsQueryable()
                .FirstOrDefaultAsync(p => p.FeeId == fee.FeeId, ct);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment not found for fee {fee.FeeId}");
            }

            // Validate amount
            if (fee.Amount != amount)
            {
                throw new InvalidOperationException(
                    $"Amount mismatch. Expected: {fee.Amount}, Received: {amount}");
            }

            // Generate simulated payment URL
            var transactionId = GenerateTransactionId();
            var paymentUrl = $"/Renter/Booking/PaymentGatewaySimulator?bookingId={bookingId}&amount={amount}&txnRef={transactionId}";

            return new SimulatedPaymentResponse
            {
                Success = true,
                PaymentUrl = paymentUrl,
                TransactionId = transactionId,
                BookingId = bookingId,
                Amount = amount,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PAYMENT SIM] Error creating payment URL for booking {BookingId}", bookingId);
            throw;
        }
    }

    /// <summary>
    /// Process simulated payment (mimics VNPay callback)
    /// Updates payment status in database
    /// </summary>
    public async Task<PaymentResultDto> ProcessPaymentAsync(
        Guid bookingId,
        string transactionId,
        PaymentSimulationResult simulationResult,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("[PAYMENT SIM] Processing payment for booking {BookingId}, result: {Result}",
                bookingId, simulationResult);

            // Get fee and payment
            var fee = await _unitOfWork.Repository<Fee>().AsQueryable()
                .FirstOrDefaultAsync(f => f.BookingId == bookingId && f.Type == FeeType.Deposit, ct);

            if (fee == null)
            {
                throw new InvalidOperationException($"Deposit fee not found for booking {bookingId}");
            }

            var payment = await _unitOfWork.Repository<Payment>().AsQueryable()
                .FirstOrDefaultAsync(p => p.FeeId == fee.FeeId, ct);

            if (payment == null)
            {
                throw new InvalidOperationException($"Payment not found for fee {fee.FeeId}");
            }

            // Check idempotency - already processed
            if (payment.Status == PaymentStatus.Paid && !string.IsNullOrEmpty(payment.ProviderReference))
            {
                _logger.LogInformation("[PAYMENT SIM] Payment already processed for booking {BookingId}", bookingId);
                return new PaymentResultDto
                {
                    Success = true,
                    BookingId = bookingId,
                    TransactionId = payment.ProviderReference,
                    Amount = payment.AmountPaid,
                    PaymentDate = payment.PaidAt ?? DateTime.UtcNow,
                    Message = "Payment already confirmed"
                };
            }

            // Update payment based on simulation result
            if (simulationResult == PaymentSimulationResult.Success)
            {
                payment.Status = PaymentStatus.Paid;
                payment.AmountPaid = fee.Amount;
                payment.PaidAt = DateTime.UtcNow;
                payment.ProviderReference = transactionId;
                payment.Method = PaymentMethod.VNPay_QR; // Simulated as VNPay QR

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogInformation("[PAYMENT SIM] Payment successful for booking {BookingId}, transaction: {TransactionId}",
                    bookingId, transactionId);

                return new PaymentResultDto
                {
                    Success = true,
                    BookingId = bookingId,
                    TransactionId = transactionId,
                    Amount = fee.Amount,
                    PaymentDate = payment.PaidAt.Value,
                    Message = "Payment successful"
                };
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ProviderReference = $"FAILED_{transactionId}";

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync(ct);

                _logger.LogWarning("[PAYMENT SIM] Payment failed for booking {BookingId}", bookingId);

                return new PaymentResultDto
                {
                    Success = false,
                    BookingId = bookingId,
                    TransactionId = transactionId,
                    Amount = fee.Amount,
                    PaymentDate = DateTime.UtcNow,
                    Message = GetFailureMessage(simulationResult)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PAYMENT SIM] Error processing payment for booking {BookingId}", bookingId);
            throw;
        }
    }

    private string GenerateTransactionId()
    {
        return $"SIM{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    private string GetFailureMessage(PaymentSimulationResult result)
    {
        return result switch
        {
            PaymentSimulationResult.InsufficientBalance => "Insufficient balance",
            PaymentSimulationResult.InvalidCard => "Invalid card",
            PaymentSimulationResult.Timeout => "Payment timeout",
            PaymentSimulationResult.UserCancelled => "User cancelled payment",
            _ => "Payment failed"
        };
    }
}

/// <summary>
/// Simulated payment response
/// </summary>
public class SimulatedPaymentResponse
{
    public bool Success { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Payment result DTO
/// </summary>
public class PaymentResultDto
{
    public bool Success { get; set; }
    public Guid BookingId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Payment simulation result
/// </summary>
public enum PaymentSimulationResult
{
    Success,
    InsufficientBalance,
    InvalidCard,
    Timeout,
    UserCancelled
}
