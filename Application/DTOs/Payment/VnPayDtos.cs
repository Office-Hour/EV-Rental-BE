namespace Application.DTOs.Payment;

/// <summary>
/// Request model for creating a VNPay payment
/// </summary>
public class VnPayPaymentRequestDto
{
    /// <summary>
    /// Unique order/booking reference ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Amount in VND (will be multiplied by 100 for VNPay)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Order description to display on payment page
    /// </summary>
    public string OrderDescription { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Specific bank/payment method code
    /// VNPAYQR: VNPay QR, VNBANK: Local ATM card, INTCARD: International card
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// Language: "vn" or "en"
    /// </summary>
    public string Locale { get; set; } = "vn";

    /// <summary>
    /// Order type: "other", "billpayment", etc.
    /// </summary>
    public string OrderType { get; set; } = "other";

    /// <summary>
    /// Client IP address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// Response model from VNPay payment creation
/// </summary>
public class VnPayPaymentResponseDto
{
    /// <summary>
    /// The generated payment URL to redirect user to VNPay
    /// </summary>
    public string PaymentUrl { get; set; } = string.Empty;

    /// <summary>
    /// The order/transaction reference
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Timestamp of payment creation
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Model for VNPay return URL callback data
/// </summary>
public class VnPayReturnDto
{
    /// <summary>
    /// Order/booking reference ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// VNPay transaction ID
    /// </summary>
    public long TransactionId { get; set; }

    /// <summary>
    /// Amount paid (divided by 100 from VNPay amount)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Bank code used for payment
    /// </summary>
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// Bank transaction number
    /// </summary>
    public string BankTranNo { get; set; } = string.Empty;

    /// <summary>
    /// Card type used
    /// </summary>
    public string CardType { get; set; } = string.Empty;

    /// <summary>
    /// VNPay response code (00 = success)
    /// </summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status (00 = success)
    /// </summary>
    public string TransactionStatus { get; set; } = string.Empty;

    /// <summary>
    /// Payment date/time
    /// </summary>
    public DateTime PayDate { get; set; }

    /// <summary>
    /// Whether payment was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Whether signature validation passed
    /// </summary>
    public bool IsValidSignature { get; set; }

    /// <summary>
    /// Order information/description
    /// </summary>
    public string OrderInfo { get; set; } = string.Empty;
}

/// <summary>
/// Model for VNPay IPN (Instant Payment Notification) response
/// </summary>
public class VnPayIpnResponseDto
{
    /// <summary>
    /// Response code: 00 = Success, 01 = Order not found, 02 = Order already confirmed,
    /// 04 = Invalid amount, 97 = Invalid signature, 99 = Unknown error
    /// </summary>
    public string RspCode { get; set; } = string.Empty;

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request model for querying VNPay transaction status
/// </summary>
public class VnPayQueryRequestDto
{
    /// <summary>
    /// Order/transaction reference ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date in VNPay format (yyyyMMddHHmmss)
    /// </summary>
    public string TransactionDate { get; set; } = string.Empty;

    /// <summary>
    /// Server IP address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// Response model from VNPay transaction query
/// </summary>
public class VnPayQueryResponseDto
{
    /// <summary>
    /// Response ID
    /// </summary>
    public string ResponseId { get; set; } = string.Empty;

    /// <summary>
    /// Command executed
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Response code (00 = success)
    /// </summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Transaction reference
    /// </summary>
    public string TxnRef { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Bank code
    /// </summary>
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// Payment date
    /// </summary>
    public string PayDate { get; set; } = string.Empty;

    /// <summary>
    /// VNPay transaction number
    /// </summary>
    public string TransactionNo { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status (00 = success)
    /// </summary>
    public string TransactionStatus { get; set; } = string.Empty;
}

/// <summary>
/// Request model for VNPay refund
/// </summary>
public class VnPayRefundRequestDto
{
    /// <summary>
    /// Order/transaction reference ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date in VNPay format (yyyyMMddHHmmss)
    /// </summary>
    public string TransactionDate { get; set; } = string.Empty;

    /// <summary>
    /// Refund amount in VND
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction type: "02" = Full refund, "03" = Partial refund
    /// </summary>
    public string TransactionType { get; set; } = "02";

    /// <summary>
    /// VNPay transaction number (optional)
    /// </summary>
    public string? TransactionNo { get; set; }

    /// <summary>
    /// User who created the refund request
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Server IP address
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// Response model from VNPay refund
/// </summary>
public class VnPayRefundResponseDto
{
    /// <summary>
    /// Response ID
    /// </summary>
    public string ResponseId { get; set; } = string.Empty;

    /// <summary>
    /// Command executed
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// Response code (00 = success)
    /// </summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Transaction reference
    /// </summary>
    public string TxnRef { get; set; } = string.Empty;

    /// <summary>
    /// Refund amount
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// VNPay transaction number
    /// </summary>
    public string TransactionNo { get; set; } = string.Empty;
}
