# VNPay Payment Integration - Implementation Summary

This document provides a quick reference for the VNPay payment gateway integration implemented in the EV Rental system.

## ?? Files Created

### Core Library & Utilities
- **`Application/Services/VNPay/VnPayLibrary.cs`**
  - Core VNPay library for parameter management and signature generation
  - `VnPayLibrary` class: Manages request/response parameters, creates signed URLs
  - `Utils` class: HMAC-SHA512 signature generation and datetime formatting

### DTOs (Data Transfer Objects)
- **`Application/DTOs/Payment/VnPayDtos.cs`**
  - `VnPayPaymentRequestDto`: Payment URL creation request
  - `VnPayPaymentResponseDto`: Payment URL creation response
  - `VnPayReturnDto`: Return URL callback data
  - `VnPayIpnResponseDto`: IPN webhook response
  - `VnPayQueryRequestDto`: Transaction query request
  - `VnPayQueryResponseDto`: Transaction query response
  - `VnPayRefundRequestDto`: Refund request
  - `VnPayRefundResponseDto`: Refund response

### Service Layer
- **`Application/Interfaces/IPaymentService.cs`**
  - Interface for payment service operations
  - Methods: CreatePaymentUrl, ProcessReturn, ProcessIPN, QueryTransaction, ProcessRefund

- **`Application/Services/PaymentService.cs`**
  - Complete implementation of IPaymentService
  - Handles all VNPay API interactions
  - Includes signature validation, order status updates, and error handling

### API Controllers
- **`WebAPI/Controllers/PaymentController.cs`**
  - RESTful endpoints for VNPay integration
  - Endpoints: `/api/payment/create`, `/api/payment/return`, `/api/payment/ipn`, `/api/payment/query`, `/api/payment/refund`
  - Comprehensive XML documentation for Swagger

## ?? Configuration

### `WebAPI/appsettings.json`

```json
{
  "VNPay": {
    "Version": "2.1.0",
    "TmnCode": "YOUR_TMN_CODE_HERE",
    "HashSecret": "YOUR_HASH_SECRET_HERE",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ApiUrl": "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction",
    "ReturnUrl": "https://yoursite.com/api/payment/return",
    "IpnUrl": "https://yoursite.com/api/payment/ipn"
  }
}
```

### Configuration Steps

1. **Get VNPay Credentials** (for production):
   - Register at https://vnpay.vn
   - Obtain `TmnCode` (Terminal/Merchant Code)
   - Obtain `HashSecret` (Secret key for HMAC-SHA512)

2. **Update Configuration**:
   - Replace `YOUR_TMN_CODE_HERE` with your merchant code
   - Replace `YOUR_HASH_SECRET_HERE` with your hash secret
   - Update `ReturnUrl` and `IpnUrl` with your actual public URLs
   - For production, update `PaymentUrl` and `ApiUrl` to production URLs

3. **Service Registration** (Already configured):
   - `PaymentService` registered in `Application/ApplicationDependencyInjection.cs`

## ?? API Endpoints

### 1. Create Payment URL
**POST** `/api/payment/create`

Creates a signed payment URL for VNPay gateway.

**Request Body:**
```json
{
  "orderId": "booking-guid-here",
  "amount": 500000,
  "orderDescription": "Deposit payment for booking ABC123",
  "bankCode": "VNPAYQR",
  "locale": "vn",
  "orderType": "other",
  "ipAddress": "127.0.0.1"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Payment URL created successfully",
  "data": {
    "paymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...",
    "orderId": "booking-guid-here",
    "amount": 500000,
    "createdAt": "2024-01-01T12:00:00Z"
  }
}
```

### 2. Process Return URL
**GET** `/api/payment/return`

Handles customer redirect after payment completion (for display purposes only).

**Query Parameters:** Automatically provided by VNPay

**Response:**
```json
{
  "success": true,
  "message": "Payment completed successfully",
  "data": {
    "orderId": "booking-guid-here",
    "transactionId": 14012345,
    "amount": 500000,
    "bankCode": "NCB",
    "responseCode": "00",
    "isSuccess": true,
    "isValidSignature": true
  }
}
```

### 3. Process IPN (Webhook)
**GET/POST** `/api/payment/ipn`

?? **CRITICAL**: Primary endpoint for updating order status.

**Query Parameters:** Automatically provided by VNPay

**Response:**
```json
{
  "rspCode": "00",
  "message": "Confirm Success"
}
```

**Response Codes:**
- `00`: Success
- `01`: Order not found
- `02`: Order already confirmed
- `04`: Invalid amount
- `97`: Invalid signature
- `99`: Unknown error

### 4. Query Transaction
**POST** `/api/payment/query` (Staff/Admin only)

Queries transaction status from VNPay.

**Request Body:**
```json
{
  "orderId": "booking-guid-here",
  "transactionDate": "20240101120000",
  "ipAddress": "127.0.0.1"
}
```

### 5. Process Refund
**POST** `/api/payment/refund` (Staff/Admin only)

Processes full or partial refund for a transaction.

**Request Body:**
```json
{
  "orderId": "booking-guid-here",
  "transactionDate": "20240101120000",
  "amount": 500000,
  "transactionType": "02",
  "transactionNo": "14012345",
  "createdBy": "staff-name",
  "ipAddress": "127.0.0.1"
}
```

**Transaction Types:**
- `02`: Full refund
- `03`: Partial refund

## ?? Security Features

### Signature Validation
- HMAC-SHA512 signature for all requests and responses
- Automatic signature validation in IPN and Return URL handlers
- Prevents tampering and ensures data integrity

### Authorization
- Public endpoints: `/create`, `/return`, `/ipn`
- Protected endpoints: `/query`, `/refund` (Staff/Admin only)
- Bearer token authentication required for protected endpoints

## ?? Integration Flow

### Standard Payment Flow

1. **Client initiates payment**:
   ```
   POST /api/payment/create
   ```

2. **Redirect user to VNPay**:
   ```
   User ? VNPay Gateway (paymentUrl from response)
   ```

3. **User completes payment on VNPay**

4. **VNPay sends IPN notification** (server-to-server):
   ```
   VNPay ? POST /api/payment/ipn
   ```
   - ? **Order status is updated here**

5. **VNPay redirects user back** (optional):
   ```
   VNPay ? GET /api/payment/return
   User sees payment result
   ```

### Key Points

- ?? **Update order status based on IPN, NOT return URL**
- Users may not reach the return URL (browser closed, network issues, etc.)
- IPN is the reliable server-to-server notification

## ?? Testing

### Test Environment
- **Payment URL**: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`
- **API URL**: `https://sandbox.vnpayment.vn/merchant_webapi/api/transaction`

### Test Card (Sandbox)
- **Bank**: NCB
- **Card Number**: `9704198526191432198`
- **Card Holder**: `NGUYEN VAN A`
- **Expiry Date**: `07/15`
- **OTP**: `123456`

### Test Scenarios

1. ? **Successful Payment**
   - Create payment ? Complete with test card ? Verify IPN received ? Check order status

2. ? **Failed Payment**
   - Create payment ? Cancel on payment page ? Verify ResponseCode 24

3. ?? **Timeout**
   - Create payment ? Don't complete within time limit ? Verify ResponseCode 11

4. ?? **Query Transaction**
   - Complete payment ? Query transaction status ? Verify response matches

5. ?? **Refund**
   - Complete payment ? Process full refund ? Verify refund success

## ?? Payment Status Tracking

### Payment Status Enum Values
- `Paid`: Payment successful
- `Refunded`: Payment refunded
- `Failed`: Payment failed

### Response Codes (Common)

| Code | Description |
|------|-------------|
| 00 | Transaction successful |
| 07 | Suspicious transaction (successful but flagged) |
| 09 | Internet Banking not registered |
| 10 | Authentication failed 3 times |
| 11 | Payment timeout |
| 12 | Account locked |
| 13 | Wrong OTP |
| 24 | Customer cancelled |
| 51 | Insufficient balance |
| 65 | Transaction limit exceeded |
| 75 | Bank under maintenance |

## ?? Logging

All payment operations are logged with structured logging:

```
[VNPAY] Creating payment URL for order {OrderId}, amount {Amount}
[VNPAY] Payment URL created successfully for order {OrderId}
[VNPAY] IPN received for order {OrderId}, amount: {Amount}, response code: {ResponseCode}
[VNPAY] Payment successful for booking {BookingId}, transaction: {TransactionId}
```

## ?? Production Checklist

Before going live:

### Configuration
- [ ] Update `TmnCode` and `HashSecret` with production credentials
- [ ] Update `PaymentUrl` to production: `https://pay.vnpay.vn/paymentv2/vpcpay.html`
- [ ] Update `ApiUrl` to production: `https://pay.vnpay.vn/merchant_webapi/api/transaction`
- [ ] Update `ReturnUrl` and `IpnUrl` to your production public URLs
- [ ] Verify IPN URL is publicly accessible from VNPay servers
- [ ] Enable HTTPS for all endpoints

### Security
- [ ] Store `HashSecret` securely (environment variables, key vault)
- [ ] Implement rate limiting on payment creation
- [ ] Add request validation
- [ ] Enable comprehensive logging

### Testing
- [ ] Test full payment flow in production sandbox
- [ ] Test IPN notification handling
- [ ] Test all error scenarios
- [ ] Test refund process
- [ ] Perform load testing

### Monitoring
- [ ] Set up logging and alerting
- [ ] Monitor IPN endpoint availability
- [ ] Track payment success/failure rates
- [ ] Monitor API response times
- [ ] Set up error tracking (e.g., Application Insights)

## ?? Troubleshooting

### Common Issues

1. **Invalid Signature Error**
   - Verify `HashSecret` is correct
   - Check parameter sorting (alphabetical)
   - Ensure URL encoding is applied correctly

2. **IPN Not Received**
   - Verify IPN URL is publicly accessible
   - Check firewall/security group settings
   - Test IPN URL manually with curl/Postman
   - Check VNPay merchant settings for IPN URL

3. **Order Not Found**
   - Verify `orderId` format (Guid)
   - Check database for booking existence
   - Ensure correct order ID is sent to VNPay

4. **Amount Mismatch**
   - Verify amount in database matches VNPay amount
   - Remember: VNPay sends amount * 100

## ?? Additional Resources

- **VNPay Sandbox**: https://sandbox.vnpayment.vn
- **Contact VNPay**:
  - Technical Support: support@vnpay.vn
  - Business Inquiry: business@vnpay.vn
  - Website: https://vnpay.vn

- **Integration Guide**: See `WebAPI/VNPAY_API_INTEGRATION_GUIDE.md`

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Based on VNPay API Version**: 2.1.0
