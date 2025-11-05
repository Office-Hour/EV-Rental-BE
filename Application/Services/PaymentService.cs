using System.Text;
using System.Text.Json;
using Application.DTOs.Payment;
using Application.Interfaces;
using Application.Services.VNPay;
using Domain.Entities.BookingManagement;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Implementation of VNPay payment gateway service
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    private string VnpVersion => _configuration["VNPay:Version"] ?? "2.1.0";
    private string VnpTmnCode => _configuration["VNPay:TmnCode"] ?? throw new InvalidOperationException("VNPay TmnCode not configured");
    private string VnpHashSecret => _configuration["VNPay:HashSecret"] ?? throw new InvalidOperationException("VNPay HashSecret not configured");
    private string VnpPaymentUrl => _configuration["VNPay:PaymentUrl"] ?? throw new InvalidOperationException("VNPay PaymentUrl not configured");
    private string VnpApiUrl => _configuration["VNPay:ApiUrl"] ?? throw new InvalidOperationException("VNPay ApiUrl not configured");
    private string VnpReturnUrl => _configuration["VNPay:ReturnUrl"] ?? throw new InvalidOperationException("VNPay ReturnUrl not configured");

    public PaymentService(
        IConfiguration configuration,
        ILogger<PaymentService> logger,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public Task<VnPayPaymentResponseDto> CreatePaymentUrlAsync(VnPayPaymentRequestDto request)
    {
        try
        {
            _logger.LogInformation("[VNPAY] Creating payment URL for order {OrderId}, amount {Amount}", 
                request.OrderId, request.Amount);

            var vnpay = new VnPayLibrary();

            // Add required parameters
            vnpay.AddRequestData("vnp_Version", VnpVersion);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", VnpTmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString()); // Convert to smallest unit
            vnpay.AddRequestData("vnp_CreateDate", Utils.GetVnPayDateTime(DateTime.Now));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", request.IpAddress);
            vnpay.AddRequestData("vnp_Locale", request.Locale);
            vnpay.AddRequestData("vnp_OrderInfo", request.OrderDescription);
            vnpay.AddRequestData("vnp_OrderType", request.OrderType);
            vnpay.AddRequestData("vnp_ReturnUrl", VnpReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", request.OrderId);

            // Optional: Specify payment method
            if (!string.IsNullOrEmpty(request.BankCode))
            {
                vnpay.AddRequestData("vnp_BankCode", request.BankCode);
            }

            // Generate signed URL
            var paymentUrl = vnpay.CreateRequestUrl(VnpPaymentUrl, VnpHashSecret);

            _logger.LogInformation("[VNPAY] Payment URL created successfully for order {OrderId}", request.OrderId);

            var response = new VnPayPaymentResponseDto
            {
                PaymentUrl = paymentUrl,
                OrderId = request.OrderId,
                Amount = request.Amount,
                CreatedAt = DateTime.UtcNow
            };

            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VNPAY] Error creating payment URL for order {OrderId}", request.OrderId);
            throw;
        }
    }

    /// <inheritdoc/>
    public Task<VnPayReturnDto> ProcessReturnUrlAsync(Dictionary<string, string> queryParams)
    {
        try
        {
            var vnpay = new VnPayLibrary();

            // Add all vnp_* parameters to library
            foreach (var (key, value) in queryParams.Where(kv => kv.Key.StartsWith("vnp_")))
            {
                vnpay.AddResponseData(key, value);
            }

            // Extract key information
            var orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnpayTranId = long.TryParse(vnpay.GetResponseData("vnp_TransactionNo"), out var tranId) ? tranId : 0;
            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var amount = long.TryParse(vnpay.GetResponseData("vnp_Amount"), out var amt) ? amt / 100m : 0;
            var bankCode = vnpay.GetResponseData("vnp_BankCode");
            var bankTranNo = vnpay.GetResponseData("vnp_BankTranNo");
            var cardType = vnpay.GetResponseData("vnp_CardType");
            var orderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var payDateStr = vnpay.GetResponseData("vnp_PayDate");
            var secureHash = queryParams.GetValueOrDefault("vnp_SecureHash", string.Empty);

            var payDate = !string.IsNullOrEmpty(payDateStr) 
                ? Utils.ParseVnPayDateTime(payDateStr) 
                : DateTime.UtcNow;

            // Validate signature
            var isValidSignature = vnpay.ValidateSignature(secureHash, VnpHashSecret);

            _logger.LogInformation("[VNPAY] Return URL processed for order {OrderId}, response code: {ResponseCode}, valid signature: {IsValid}",
                orderId, responseCode, isValidSignature);

            var result = new VnPayReturnDto
            {
                OrderId = orderId,
                TransactionId = vnpayTranId,
                Amount = amount,
                BankCode = bankCode,
                BankTranNo = bankTranNo,
                CardType = cardType,
                ResponseCode = responseCode,
                TransactionStatus = transactionStatus,
                PayDate = payDate,
                IsSuccess = responseCode == "00" && transactionStatus == "00",
                IsValidSignature = isValidSignature,
                OrderInfo = orderInfo
            };

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VNPAY] Error processing return URL");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<VnPayIpnResponseDto> ProcessIpnAsync(Dictionary<string, string> queryParams)
    {
        try
        {
            var vnpay = new VnPayLibrary();

            // Add all vnp_* parameters
            foreach (var (key, value) in queryParams.Where(kv => kv.Key.StartsWith("vnp_")))
            {
                vnpay.AddResponseData(key, value);
            }

            // Extract information
            var orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnpAmount = long.TryParse(vnpay.GetResponseData("vnp_Amount"), out var amt) ? amt / 100m : 0;
            var vnpayTranId = long.TryParse(vnpay.GetResponseData("vnp_TransactionNo"), out var tranId) ? tranId : 0;
            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var bankCode = vnpay.GetResponseData("vnp_BankCode");
            var payDateStr = vnpay.GetResponseData("vnp_PayDate");
            var secureHash = queryParams.GetValueOrDefault("vnp_SecureHash", string.Empty);

            _logger.LogInformation("[VNPAY] IPN received for order {OrderId}, amount: {Amount}, response code: {ResponseCode}",
                orderId, vnpAmount, responseCode);

            // Validate signature
            var isValid = vnpay.ValidateSignature(secureHash, VnpHashSecret);
            if (!isValid)
            {
                _logger.LogWarning("[VNPAY] Invalid signature for order {OrderId}", orderId);
                return new VnPayIpnResponseDto
                {
                    RspCode = "97",
                    Message = "Invalid signature"
                };
            }

            // Parse order ID as Guid (assuming it's a BookingId)
            if (!Guid.TryParse(orderId, out var bookingId))
            {
                _logger.LogWarning("[VNPAY] Invalid order ID format: {OrderId}", orderId);
                return new VnPayIpnResponseDto
                {
                    RspCode = "01",
                    Message = "Order not found"
                };
            }

            // Get booking from database
            var booking = await _unitOfWork.Repository<Booking>()
                .GetByIdAsync(bookingId);

            if (booking == null)
            {
                _logger.LogWarning("[VNPAY] Booking not found: {BookingId}", bookingId);
                return new VnPayIpnResponseDto
                {
                    RspCode = "01",
                    Message = "Order not found"
                };
            }

            // Get deposit fee
            var fee = await _unitOfWork.Repository<Fee>().AsQueryable()
                .FirstOrDefaultAsync(f => f.BookingId == bookingId && f.Type == FeeType.Deposit);

            if (fee == null)
            {
                _logger.LogWarning("[VNPAY] Deposit fee not found for booking: {BookingId}", bookingId);
                return new VnPayIpnResponseDto
                {
                    RspCode = "01",
                    Message = "Fee not found"
                };
            }

            // Validate amount
            if (fee.Amount != vnpAmount)
            {
                _logger.LogWarning("[VNPAY] Amount mismatch for booking {BookingId}. Expected: {Expected}, Received: {Received}",
                    bookingId, fee.Amount, vnpAmount);
                return new VnPayIpnResponseDto
                {
                    RspCode = "04",
                    Message = "Invalid amount"
                };
            }

            // Get payment
            var payment = await _unitOfWork.Repository<Payment>().AsQueryable()
                .FirstOrDefaultAsync(p => p.FeeId == fee.FeeId);

            if (payment == null)
            {
                _logger.LogWarning("[VNPAY] Payment not found for fee: {FeeId}", fee.FeeId);
                return new VnPayIpnResponseDto
                {
                    RspCode = "01",
                    Message = "Payment not found"
                };
            }

            // Check if already processed (idempotency)
            if (payment.Status == PaymentStatus.Paid && !string.IsNullOrEmpty(payment.ProviderReference))
            {
                _logger.LogInformation("[VNPAY] Payment already processed for booking {BookingId}", bookingId);
                return new VnPayIpnResponseDto
                {
                    RspCode = "02",
                    Message = "Order already confirmed"
                };
            }

            // Update payment status based on VNPay response
            if (responseCode == "00" && transactionStatus == "00")
            {
                payment.Status = PaymentStatus.Paid;
                payment.AmountPaid = vnpAmount;
                payment.ProviderReference = vnpayTranId.ToString();
                payment.Method = MapBankCodeToPaymentMethod(bankCode);
                
                if (!string.IsNullOrEmpty(payDateStr))
                {
                    payment.PaidAt = Utils.ParseVnPayDateTime(payDateStr);
                }

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("[VNPAY] Payment successful for booking {BookingId}, transaction: {TransactionId}",
                    bookingId, vnpayTranId);

                return new VnPayIpnResponseDto
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                };
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.ProviderReference = $"FAILED_{responseCode}";

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogWarning("[VNPAY] Payment failed for booking {BookingId}, response code: {ResponseCode}",
                    bookingId, responseCode);

                return new VnPayIpnResponseDto
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VNPAY] Error processing IPN");
            return new VnPayIpnResponseDto
            {
                RspCode = "99",
                Message = "Unknown error"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<VnPayQueryResponseDto> QueryTransactionAsync(VnPayQueryRequestDto request)
    {
        try
        {
            _logger.LogInformation("[VNPAY] Querying transaction for order {OrderId}", request.OrderId);

            var vnpRequestId = DateTime.Now.Ticks.ToString();
            var vnpCreateDate = Utils.GetVnPayDateTime(DateTime.Now);

            // Create signature
            var signData = $"{vnpRequestId}|{VnpVersion}|querydr|{VnpTmnCode}|{request.OrderId}|{request.TransactionDate}|{vnpCreateDate}|{request.IpAddress}|Query transaction: {request.OrderId}";
            var vnpSecureHash = Utils.HmacSHA512(VnpHashSecret, signData);

            // Create request object
            var requestData = new
            {
                vnp_RequestId = vnpRequestId,
                vnp_Version = VnpVersion,
                vnp_Command = "querydr",
                vnp_TmnCode = VnpTmnCode,
                vnp_TxnRef = request.OrderId,
                vnp_OrderInfo = $"Query transaction: {request.OrderId}",
                vnp_TransactionDate = request.TransactionDate,
                vnp_CreateDate = vnpCreateDate,
                vnp_IpAddr = request.IpAddress,
                vnp_SecureHash = vnpSecureHash
            };

            // Send POST request
            using var httpClient = new HttpClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(VnpApiUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[VNPAY] Query transaction response: {Response}", responseContent);

            var queryResponse = JsonSerializer.Deserialize<VnPayQueryResponseDto>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return queryResponse ?? new VnPayQueryResponseDto { ResponseCode = "99", Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VNPAY] Error querying transaction for order {OrderId}", request.OrderId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<VnPayRefundResponseDto> ProcessRefundAsync(VnPayRefundRequestDto request)
    {
        try
        {
            _logger.LogInformation("[VNPAY] Processing refund for order {OrderId}, amount: {Amount}",
                request.OrderId, request.Amount);

            var vnpRequestId = DateTime.Now.Ticks.ToString();
            var vnpAmount = (long)(request.Amount * 100);
            var vnpCreateDate = Utils.GetVnPayDateTime(DateTime.Now);
            var vnpTransactionNo = request.TransactionNo ?? string.Empty;

            // Create signature
            var signData = $"{vnpRequestId}|{VnpVersion}|refund|{VnpTmnCode}|{request.TransactionType}|{request.OrderId}|{vnpAmount}|{vnpTransactionNo}|{request.TransactionDate}|{request.CreatedBy}|{vnpCreateDate}|{request.IpAddress}|Refund for order: {request.OrderId}";
            var vnpSecureHash = Utils.HmacSHA512(VnpHashSecret, signData);

            // Create request object
            var requestData = new
            {
                vnp_RequestId = vnpRequestId,
                vnp_Version = VnpVersion,
                vnp_Command = "refund",
                vnp_TmnCode = VnpTmnCode,
                vnp_TransactionType = request.TransactionType,
                vnp_TxnRef = request.OrderId,
                vnp_Amount = vnpAmount,
                vnp_OrderInfo = $"Refund for order: {request.OrderId}",
                vnp_TransactionNo = vnpTransactionNo,
                vnp_TransactionDate = request.TransactionDate,
                vnp_CreateBy = request.CreatedBy,
                vnp_CreateDate = vnpCreateDate,
                vnp_IpAddr = request.IpAddress,
                vnp_SecureHash = vnpSecureHash
            };

            // Send POST request
            using var httpClient = new HttpClient();
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(VnpApiUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[VNPAY] Refund response: {Response}", responseContent);

            var refundResponse = JsonSerializer.Deserialize<VnPayRefundResponseDto>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return refundResponse ?? new VnPayRefundResponseDto { ResponseCode = "99", Message = "Failed to parse response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VNPAY] Error processing refund for order {OrderId}", request.OrderId);
            throw;
        }
    }

    /// <summary>
    /// Map VNPay bank code to PaymentMethod enum
    /// </summary>
    private static PaymentMethod MapBankCodeToPaymentMethod(string bankCode)
    {
        return bankCode?.ToUpper() switch
        {
            "VNPAYQR" => PaymentMethod.VNPay_QR,
            "VNBANK" => PaymentMethod.Bank_Transfer,
            "INTCARD" => PaymentMethod.International_Card,
            _ => PaymentMethod.Unknown
        };
    }
}
