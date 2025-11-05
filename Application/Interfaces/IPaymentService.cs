using Application.DTOs.Payment;

namespace Application.Interfaces;

/// <summary>
/// Service interface for VNPay payment gateway integration
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create a payment URL for VNPay gateway
    /// </summary>
    /// <param name="request">Payment request details</param>
    /// <returns>Payment response with URL to redirect user</returns>
    Task<VnPayPaymentResponseDto> CreatePaymentUrlAsync(VnPayPaymentRequestDto request);

    /// <summary>
    /// Process return URL callback from VNPay after customer completes payment
    /// </summary>
    /// <param name="queryParams">Query string parameters from VNPay</param>
    /// <returns>Processed payment return data</returns>
    Task<VnPayReturnDto> ProcessReturnUrlAsync(Dictionary<string, string> queryParams);

    /// <summary>
    /// Process IPN (Instant Payment Notification) webhook from VNPay
    /// This is the primary method for updating order status
    /// </summary>
    /// <param name="queryParams">Query string parameters from VNPay</param>
    /// <returns>IPN response to send back to VNPay</returns>
    Task<VnPayIpnResponseDto> ProcessIpnAsync(Dictionary<string, string> queryParams);

    /// <summary>
    /// Query transaction status from VNPay
    /// </summary>
    /// <param name="request">Query request details</param>
    /// <returns>Transaction status information</returns>
    Task<VnPayQueryResponseDto> QueryTransactionAsync(VnPayQueryRequestDto request);

    /// <summary>
    /// Process refund for a transaction
    /// </summary>
    /// <param name="request">Refund request details</param>
    /// <returns>Refund response information</returns>
    Task<VnPayRefundResponseDto> ProcessRefundAsync(VnPayRefundRequestDto request);
}
