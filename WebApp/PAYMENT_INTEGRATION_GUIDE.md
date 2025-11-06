# WebApp Payment Integration Guide

## ?? T?ng Quan

Document này h??ng d?n c?u hình và s? d?ng lu?ng Payment tích h?p VNPay cho WebApp (Razor Pages).

---

## ??? Ki?n Trúc Lu?ng Payment

### Lu?ng Hoàn Ch?nh

```
???????????????????????????????????????????????????????????????????????
?                         BOOKING + PAYMENT FLOW                       ?
???????????????????????????????????????????????????????????????????????

1. User: Create.cshtml
   ??> Ch?n vehicle + th?i gian thuê
   ??> TempData["BookingData"] = CreateBookingDto

2. User: Payment.cshtml
   ??> Ch?n payment method (VNPay QR/Bank/Card)
   ??> Tính deposit amount (20% estimated cost)
   ??> TempData["PaymentData"] = DepositFeeDto (status = Pending)

3. User: Confirm.cshtml
   ??> Review thông tin
   ??> POST to Confirm.OnPostAsync()
   ?   ??> Call API POST /api/booking (CreateBookingCommand)
   ?   ?   ??> T?o Booking (status = Pending_Verification)
   ?   ?   ??> T?o Fee (type = Deposit)
   ?   ?   ??> T?o Payment (status = Pending, AmountPaid = 0)
   ?   ??> Return BookingId
   ??> Redirect to ProcessPayment.cshtml?bookingId={id}&amount={amount}

4. System: ProcessPayment.cshtml.OnGetAsync()
   ??> Call API POST /api/payment/create
   ?   ??> Create VNPay payment URL (signed)
   ?   ??> Return paymentUrl
   ??> Redirect to VNPay Gateway

5. User: VNPay Gateway
   ??> User selects bank/card
   ??> User completes payment
   ??> VNPay processes transaction

6. VNPay: IPN Webhook (Server-to-Server)
   ??> POST/GET /api/payment/ipn?vnp_*=...
   ??> Validate signature
   ??> Update Payment.Status = Paid
   ??> Update Payment.AmountPaid = vnpAmount
   ??> Update Payment.ProviderReference = vnp_TransactionNo
   ??> Return {RspCode: "00", Message: "Confirm Success"}

7. VNPay: Return URL (User Redirect)
   ??> GET /Renter/Booking/PaymentResult?vnp_*=...
   ??> Call API GET /api/payment/return?vnp_*=...
   ??> Validate signature + display result
   ??> Redirect to RenterProfile or Retry payment

8. User: Check booking status
   ??> Payment.Status = Paid ? Waiting for staff verification
```

---

## ?? Files Changed/Created

### ? Created Files

| File | Purpose |
|------|---------|
| `WebApp/Services/ApiClient.cs` | HTTP client wrapper for calling backend API |
| `WebApp/Models/Api/PaymentModels.cs` | DTOs for API communication |
| `WebApp/Areas/Renter/Pages/Booking/ProcessPayment.cshtml.cs` | Redirect to VNPay gateway |
| `WebApp/Areas/Renter/Pages/Booking/ProcessPayment.cshtml` | Processing UI |
| `WebApp/Areas/Renter/Pages/Booking/PaymentResult.cshtml.cs` | Handle VNPay return URL |
| `WebApp/Areas/Renter/Pages/Booking/PaymentResult.cshtml` | Display payment result |

### ? Modified Files

| File | Changes |
|------|---------|
| `WebApp/Areas/Renter/Pages/Booking/Payment.cshtml.cs` | ? Removed mock payment logic<br>? Set AmountPaid = 0, ProviderReference = null |
| `WebApp/Areas/Renter/Pages/Booking/Confirm.cshtml.cs` | ? Call API to create booking<br>? Redirect to ProcessPayment instead of Profile |
| `WebApp/Areas/Renter/Pages/Booking/Confirm.cshtml` | ? Changed PaidAt to CreatedAt |
| `WebApp/Program.cs` | ? Register HttpClient<ApiClient><br>? Configure API base URL |

---

## ?? Configuration

### 1. appsettings.json (WebApp)

Create or update `WebApp/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001"
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String_Here"
  }
}
```

### 2. appsettings.Development.json (WebApp)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

### 3. appsettings.json (WebAPI)

Update `WebAPI/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Application.Services.PaymentService": "Debug"
    }
  },
  
  "AllowedHosts": "*",
  
  "VNPay": {
    "Version": "2.1.0",
    "TmnCode": "YOUR_TMN_CODE_HERE",
    "HashSecret": "YOUR_HASH_SECRET_HERE",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ApiUrl": "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction",
    "ReturnUrl": "https://localhost:5001/Renter/Booking/PaymentResult",
    "IpnUrl": "https://YOUR_PUBLIC_URL/api/payment/ipn"
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String_Here"
  }
}
```

**?? IMPORTANT:**
- `ReturnUrl`: URL mà VNPay redirect user sau khi thanh toán (WebApp Razor Page)
- `IpnUrl`: URL mà VNPay g?i webhook (WebAPI endpoint, ph?i public accessible)

---

## ?? Testing Flow

### Step 1: Start Both Projects

```bash
# Terminal 1 - Start WebAPI
cd WebAPI
dotnet run

# Terminal 2 - Start WebApp
cd WebApp
dotnet run

# Terminal 3 - Expose WebAPI to public (for IPN)
ngrok http 7001
```

### Step 2: Update VNPay IPN URL

1. Copy ngrok public URL (e.g., `https://abc123.ngrok.io`)
2. Update `WebAPI/appsettings.json`:
   ```json
   "IpnUrl": "https://abc123.ngrok.io/api/payment/ipn"
   ```
3. Restart WebAPI

### Step 3: Test Booking Flow

1. Navigate to: `https://localhost:5001` (WebApp)
2. Login as Renter
3. Go to Create Booking page
4. Select vehicle + time range
5. Click "Ti?p Theo" ? redirects to Payment page
6. Select payment method (VNPay QR recommended)
7. Click "Ti?p Theo" ? redirects to Confirm page
8. Review information
9. Check "?i?u kho?n d?ch v?"
10. Click "Xác Nh?n ??t Ch?"

**Expected:**
- Booking created with `Status = Pending_Verification`
- Payment created with `Status = Pending`, `AmountPaid = 0`
- Redirects to ProcessPayment page
- Immediately redirects to VNPay gateway

### Step 4: Complete Payment on VNPay

**VNPay Sandbox Test Cards:**

| Bank Code | Card Number | Card Holder | Expiry | CVV | OTP |
|-----------|-------------|-------------|--------|-----|-----|
| NCB | 9704198526191432198 | NGUYEN VAN A | 07/15 | 123 | 123456 |

**Steps:**
1. On VNPay page, select payment method
2. Enter test card details
3. Enter OTP: `123456`
4. Click "Thanh Toán"

**Expected:**
- IPN webhook called to update payment
- User redirected back to PaymentResult page
- Display success message with transaction details

### Step 5: Verify Payment in Database

```sql
SELECT 
    b.BookingId,
    b.Status AS BookingStatus,
    f.Amount AS DepositAmount,
    p.Status AS PaymentStatus,
    p.AmountPaid,
    p.ProviderReference,
    p.PaidAt
FROM Bookings b
JOIN Fees f ON b.BookingId = f.BookingId
JOIN Payments p ON f.FeeId = p.FeeId
WHERE b.RenterId = 'YOUR_RENTER_ID'
ORDER BY b.CreatedAt DESC;
```

**Expected Results:**
- `BookingStatus` = `Pending_Verification`
- `PaymentStatus` = `Paid`
- `AmountPaid` = Deposit amount
- `ProviderReference` = VNPay transaction ID
- `PaidAt` = Payment timestamp

---

## ?? Testing Scenarios

### ? Happy Path

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create booking + select VNPay | Booking created with Payment.Status = Pending |
| 2 | Complete payment on VNPay | IPN updates Payment.Status = Paid |
| 3 | Return to WebApp | PaymentResult shows success |
| 4 | Check booking in profile | Booking shows "Ch? Xác Minh" |

### ? Failure Scenarios

| Scenario | Action | Expected Result |
|----------|--------|-----------------|
| User cancels payment | Click "H?y" on VNPay | Return URL shows failure, can retry |
| Payment timeout | Wait 15 minutes | Return URL shows timeout error |
| Invalid bank card | Enter wrong card | VNPay shows error, can retry |
| Network error during payment | Disconnect internet | Show error, can retry from booking list |
| IPN not received | Firewall blocks ngrok | Payment stays Pending, use query API to check |

### ?? Retry Payment

If payment fails, user can retry from booking details:

```
RenterProfile ? View Bookings ? Find pending booking ? Click "Thanh Toán L?i"
? Redirects to ProcessPayment with existing BookingId
? Creates new VNPay payment URL
? User can complete payment
```

---

## ?? Security Checklist

### ? Authentication

- [ ] All booking pages require `[Authorize(Roles = "Renter")]`
- [ ] API endpoints require Bearer token
- [ ] RenterId extracted from token, not from request body

### ? Payment Security

- [ ] VNPay signature validated on both Return URL and IPN
- [ ] Payment amount validated against Fee amount in database
- [ ] Idempotency: duplicate IPN calls don't double-update
- [ ] TempData cleared after booking creation

### ? Data Integrity

- [ ] Payment.Status = Pending on creation
- [ ] Only IPN can update Payment.Status to Paid
- [ ] Return URL is read-only (display only)
- [ ] Transaction boundaries for multi-write operations

---

## ?? Monitoring & Logging

### Key Log Points

```csharp
// WebApp - Confirm.cshtml.cs
_logger.LogInformation("Booking created successfully: {BookingId}", bookingId);

// WebApp - ProcessPayment.cshtml.cs
_logger.LogInformation("Creating VNPay payment URL for booking {BookingId}", BookingId);
_logger.LogInformation("VNPay payment URL created: {PaymentUrl}", response.Data.PaymentUrl);

// WebAPI - PaymentService
_logger.LogInformation("[VNPAY] Creating payment URL for order {OrderId}, amount {Amount}", ...);
_logger.LogInformation("[VNPAY] IPN received for order {OrderId}, amount: {Amount}, response code: {ResponseCode}", ...);
_logger.LogInformation("[VNPAY] Payment successful for booking {BookingId}, transaction: {TransactionId}", ...);
```

### Metrics to Track

- Total bookings created
- Payment success rate
- Payment failure rate by error code
- Average time from booking to payment completion
- IPN webhook latency
- Return URL redirect rate

---

## ?? Troubleshooting

### Problem: IPN not received

**Symptoms:**
- Payment completed on VNPay
- Return URL shows success
- But Payment.Status still Pending

**Diagnosis:**
```bash
# Check ngrok tunnel is running
curl https://your-ngrok-url.ngrok.io/api/payment/ipn

# Check WebAPI logs
dotnet run --project WebAPI
# Look for: "[VNPAY] IPN received..."
```

**Solutions:**
1. Verify ngrok is running: `ngrok http 7001`
2. Update `appsettings.json` IpnUrl with ngrok URL
3. Restart WebAPI
4. Check firewall/antivirus not blocking incoming requests
5. Test IPN endpoint manually:
   ```bash
   curl "https://your-ngrok-url.ngrok.io/api/payment/ipn?test=true"
   ```

### Problem: "Không th? t?o liên k?t thanh toán VNPay"

**Symptoms:**
- ProcessPayment page shows error
- Cannot redirect to VNPay

**Diagnosis:**
- Check WebAPI is running on correct port
- Check `ApiSettings:BaseUrl` in WebApp/appsettings.json
- Check Bearer token in cookies

**Solutions:**
1. Verify WebAPI running: `curl https://localhost:7001/api/payment/create`
2. Check logs in WebApp for API call errors
3. Verify authentication token exists in browser cookies

### Problem: Payment amount mismatch

**Symptoms:**
- IPN returns RspCode = "04" (Invalid amount)

**Diagnosis:**
```sql
SELECT 
    f.Amount AS FeeAmount,
    p.AmountPaid AS PaymentAmount,
    p.ProviderReference
FROM Fees f
JOIN Payments p ON f.FeeId = p.FeeId
WHERE f.BookingId = 'BOOKING_ID';
```

**Solutions:**
- Ensure deposit calculation is consistent
- Use `Math.Round(..., 2, MidpointRounding.AwayFromZero)`
- VNPay amount is in smallest unit (multiply by 100)

---

## ?? Next Steps

### Phase 1: Complete Basic Flow ?
- [x] Create booking with payment pending
- [x] Redirect to VNPay
- [x] Handle IPN webhook
- [x] Handle return URL
- [x] Display payment result

### Phase 2: Enhanced Features
- [ ] Add retry payment from booking list
- [ ] Add payment history view
- [ ] Add email notification after payment
- [ ] Add SMS notification with booking details
- [ ] Add payment receipt download (PDF)

### Phase 3: Production Readiness
- [ ] Switch to VNPay production credentials
- [ ] Deploy backend to public server (remove ngrok)
- [ ] Configure production Return URL
- [ ] Configure production IPN URL
- [ ] Add rate limiting for payment creation
- [ ] Add audit logging for payment status changes
- [ ] Add payment reconciliation job

---

## ?? Summary Checklist

Before deploying to production:

### WebApp
- [ ] `ApiSettings:BaseUrl` points to production API
- [ ] Authentication configured correctly
- [ ] TempData provider configured (Redis for distributed scenarios)
- [ ] Error handling for all API calls
- [ ] Loading states for async operations

### WebAPI
- [ ] VNPay production credentials configured
- [ ] `ReturnUrl` points to production WebApp
- [ ] `IpnUrl` points to public production API
- [ ] HTTPS enabled
- [ ] CORS configured for WebApp origin
- [ ] Logging configured (Application Insights/Serilog)
- [ ] Database connection string secured (Azure Key Vault)

### Database
- [ ] Payment.Status has Pending value
- [ ] Payment.PaidAt is nullable
- [ ] Indexes on Booking.RenterId, Payment.FeeId
- [ ] RowVersion for concurrency control

### Testing
- [ ] E2E test: Create ? Pay ? Success
- [ ] E2E test: Create ? Pay ? Failure ? Retry
- [ ] E2E test: IPN idempotency (duplicate calls)
- [ ] Load test: 100 concurrent payments
- [ ] Security test: Invalid signature rejection

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**WebApp**: Razor Pages (.NET 9)  
**WebAPI**: ASP.NET Core (.NET 9)  
**VNPay Version**: 2.1.0
