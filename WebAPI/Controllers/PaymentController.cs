using Application.DTOs.Payment;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Responses;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a VNPay payment URL for a booking
    /// </summary>
    /// <param name="request">Payment request details</param>
    /// <returns>Payment URL to redirect user to VNPay gateway</returns>
    /// <response code="200">Payment URL created successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <remarks>
    /// This endpoint creates a signed payment URL for VNPay gateway.
    /// The client should redirect the user to the returned payment URL.
    /// 
    /// Example request:
    /// 
    ///     POST /api/payment/create
    ///     {
    ///         "orderId": "booking-guid-here",
    ///         "amount": 500000,
    ///         "orderDescription": "Deposit payment for booking ABC123",
    ///         "bankCode": "VNPAYQR",
    ///         "locale": "vn",
    ///         "orderType": "other",
    ///         "ipAddress": "127.0.0.1"
    ///     }
    /// 
    /// </remarks>
    [HttpPost("create")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<VnPayPaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<VnPayPaymentResponseDto>>> CreatePayment(
        [FromBody] VnPayPaymentRequestDto request)
    {
        try
        {
            // Get client IP if not provided
            if (string.IsNullOrEmpty(request.IpAddress))
            {
                request.IpAddress = IpAddressHelper.GetClientIpAddress(HttpContext);
            }

            var result = await _paymentService.CreatePaymentUrlAsync(request);

            return Ok(new ApiResponse<VnPayPaymentResponseDto>(result, "Payment URL created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment URL for order {OrderId}", request.OrderId);
            return BadRequest(new ErrorMessage { Message = "Failed to create payment URL" });
        }
    }

    /// <summary>
    /// Process VNPay return URL callback after customer completes payment
    /// </summary>
    /// <returns>Payment result information for display</returns>
    /// <response code="200">Payment processed successfully</response>
    /// <response code="400">Invalid signature or payment data</response>
    /// <remarks>
    /// This endpoint handles the customer redirect from VNPay after payment completion.
    /// It validates the signature and returns payment details for display purposes.
    /// 
    /// ?? IMPORTANT: This endpoint is for DISPLAY purposes only. Do not update order status here.
    /// Use the IPN endpoint for reliable order status updates.
    /// 
    /// Query parameters are automatically passed by VNPay and include:
    /// - vnp_TxnRef: Order reference
    /// - vnp_Amount: Amount paid
    /// - vnp_ResponseCode: Response code (00 = success)
    /// - vnp_TransactionNo: VNPay transaction ID
    /// - vnp_SecureHash: Signature for validation
    /// - and many more...
    /// </remarks>
    [HttpGet("return")]
    [ProducesResponseType(typeof(ApiResponse<VnPayReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VnPayReturnDto>>> ProcessReturn()
    {
        try
        {
            // Convert query string to dictionary
            var queryParams = Request.Query.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToString()
            );

            var result = await _paymentService.ProcessReturnUrlAsync(queryParams);

            if (!result.IsValidSignature)
            {
                _logger.LogWarning("[VNPAY] Invalid signature for order {OrderId}", result.OrderId);
                return BadRequest(new ErrorMessage { Message = "Invalid payment signature" });
            }

            var message = result.IsSuccess
                ? "Payment completed successfully"
                : $"Payment failed with code: {result.ResponseCode}";

            return Ok(new ApiResponse<VnPayReturnDto>(result, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay return URL");
            return BadRequest(new ErrorMessage { Message = "Failed to process payment return" });
        }
    }

    /// <summary>
    /// Process VNPay IPN (Instant Payment Notification) webhook
    /// </summary>
    /// <returns>IPN response for VNPay</returns>
    /// <response code="200">IPN processed successfully</response>
    /// <remarks>
    /// ?? CRITICAL: This is the PRIMARY endpoint for updating order/payment status.
    /// 
    /// This endpoint must:
    /// - Be publicly accessible from VNPay servers
    /// - Process idempotently (handle duplicate notifications)
    /// - Return response within 30 seconds
    /// - Validate signature before processing
    /// 
    /// Response codes:
    /// - 00: Success
    /// - 01: Order not found
    /// - 02: Order already confirmed
    /// - 04: Invalid amount
    /// - 97: Invalid signature
    /// - 99: Unknown error
    /// 
    /// VNPay will call this endpoint automatically after payment completion.
    /// Configure the IPN URL in your VNPay merchant settings or appsettings.json.
    /// </remarks>
    [HttpGet("ipn")]
    [HttpPost("ipn")]
    [ProducesResponseType(typeof(VnPayIpnResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VnPayIpnResponseDto>> ProcessIpn()
    {
        try
        {
            // Convert query string to dictionary (VNPay sends data as query params even for POST)
            var queryParams = Request.Query.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToString()
            );

            var result = await _paymentService.ProcessIpnAsync(queryParams);

            // Always return 200 OK with the response code in the body
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay IPN");
            return Ok(new VnPayIpnResponseDto
            {
                RspCode = "99",
                Message = "Unknown error"
            });
        }
    }

    /// <summary>
    /// Query transaction status from VNPay
    /// </summary>
    /// <param name="request">Query request details</param>
    /// <returns>Transaction status information</returns>
    /// <response code="200">Transaction status retrieved successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized - Bearer token required (Staff only)</response>
    /// <remarks>
    /// This endpoint queries the current status of a transaction from VNPay.
    /// 
    /// Example request:
    /// 
    ///     POST /api/payment/query
    ///     {
    ///         "orderId": "booking-guid-here",
    ///         "transactionDate": "20240101120000",
    ///         "ipAddress": "127.0.0.1"
    ///     }
    /// 
    /// The transactionDate should be in VNPay format (yyyyMMddHHmmss).
    /// </remarks>
    [HttpPost("query")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<VnPayQueryResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<VnPayQueryResponseDto>>> QueryTransaction(
        [FromBody] VnPayQueryRequestDto request)
    {
        try
        {
            // Get server IP if not provided
            if (string.IsNullOrEmpty(request.IpAddress))
            {
                request.IpAddress = IpAddressHelper.GetServerIpAddress(HttpContext);
            }

            var result = await _paymentService.QueryTransactionAsync(request);

            var message = result.ResponseCode == "00"
                ? "Transaction status retrieved successfully"
                : $"Query failed: {result.Message}";

            return Ok(new ApiResponse<VnPayQueryResponseDto>(result, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying transaction {OrderId}", request.OrderId);
            return BadRequest(new ErrorMessage { Message = "Failed to query transaction" });
        }
    }

    /// <summary>
    /// Process refund for a transaction
    /// </summary>
    /// <param name="request">Refund request details</param>
    /// <returns>Refund response information</returns>
    /// <response code="200">Refund processed successfully</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">Unauthorized - Bearer token required (Staff only)</response>
    /// <remarks>
    /// This endpoint processes a full or partial refund for a transaction.
    /// 
    /// Transaction types:
    /// - "02": Full refund
    /// - "03": Partial refund
    /// 
    /// Example request:
    /// 
    ///     POST /api/payment/refund
    ///     {
    ///         "orderId": "booking-guid-here",
    ///         "transactionDate": "20240101120000",
    ///         "amount": 500000,
    ///         "transactionType": "02",
    ///         "transactionNo": "14012345",
    ///         "createdBy": "staff-name",
    ///         "ipAddress": "127.0.0.1"
    ///     }
    /// 
    /// </remarks>
    [HttpPost("refund")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<VnPayRefundResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<VnPayRefundResponseDto>>> ProcessRefund(
        [FromBody] VnPayRefundRequestDto request)
    {
        try
        {
            // Get server IP if not provided
            if (string.IsNullOrEmpty(request.IpAddress))
            {
                request.IpAddress = IpAddressHelper.GetServerIpAddress(HttpContext);
            }

            var result = await _paymentService.ProcessRefundAsync(request);

            var message = result.ResponseCode == "00"
                ? "Refund processed successfully"
                : $"Refund failed: {result.Message}";

            return Ok(new ApiResponse<VnPayRefundResponseDto>(result, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for order {OrderId}", request.OrderId);
            return BadRequest(new ErrorMessage { Message = "Failed to process refund" });
        }
    }
}
