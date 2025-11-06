# WebApp Standalone Payment Integration Guide

## ?? T?ng Quan

Document này h??ng d?n s? d?ng **WebApp Standalone Payment System** - m?t h? th?ng thanh toán ??c l?p không ph? thu?c vào WebAPI, s? d?ng **Payment Simulation Service** ?? gi? l?p VNPay gateway và c?p nh?t database th?t.

---

## ?? ??c ?i?m H? Th?ng

### ? Standalone System
- **KHÔNG** ph? thu?c vào WebAPI
- G?i **MediatR Commands/Queries tr?c ti?p**
- S? d?ng **Clean Architecture** layers (Application, Domain, Persistence)
- **Shared database** v?i WebAPI (có th?)

### ? Payment Simulation
- **PaymentSimulationService** gi? l?p VNPay gateway
- Update **Payment.Status trong database th?t**
- H? tr? nhi?u **test scenarios** (Success, Failed, Timeout, etc.)
- **Idempotent** payment processing

### ? Business Rules Compliance
- Tuân th? business rules t? `.github/copilot-instructions.md`
- Booking ? Payment (Pending) ? PaymentGateway ? Payment (Paid/Failed)
- **Status transitions** enforced
- **95% refund** on cancellation (5% transaction fee)

---

## ??? Ki?n Trúc Lu?ng

### Lu?ng Hoàn Ch?nh (Standalone)

```
???????????????????????????????????????????????????????????????????????
?                  STANDALONE BOOKING + PAYMENT FLOW                   ?
???????????????????????????????????????????????????????????????????????

1. User: Create.cshtml
   ??> Ch?n vehicle + th?i gian thuê
   ??> TempData["BookingData"] = CreateBookingDto

2. User: Payment.cshtml
   ??> Ch?n payment method (VNPay QR - simulated)
   ??> Tính deposit amount (20% estimated cost)
   ??> TempData["PaymentData"] = DepositFeeDto (AmountPaid = 0)

3. User: Confirm.cshtml
   ??> Review thông tin
   ??> POST to Confirm.OnPostAsync()
   ?   ??> MediatR: CreateBookingCommand
   ?   ?   ??> T?o Booking (status = Pending_Verification)
   ?   ?   ??> T?o Fee (type = Deposit)
   ?   ?   ??> T?o Payment (status = Pending, AmountPaid = 0)
   ?   ??> Return BookingId
   ??> Redirect to ProcessPayment.cshtml?bookingId={id}

4. System: ProcessPayment.cshtml.OnGetAsync()
   ??> PaymentSimulationService.CreatePaymentUrlAsync()
   ?   ??> Validate booking + fee + payment
   ?   ??> Generate transaction ID
   ?   ??> Return simulated payment URL
   ??> Redirect to PaymentGatewaySimulator.cshtml

5. User: PaymentGatewaySimulator.cshtml
   ??> User selects simulation result:
   ?   • Success ?
   ?   • InsufficientBalance ?
   ?   • InvalidCard ?
   ?   • Timeout ??
   ?   • UserCancelled ??
   ??> POST to PaymentGatewaySimulator.OnPostAsync()

6. System: PaymentGatewaySimulator.OnPostAsync()
   ??> Simulate delay (2 seconds)
   ??> PaymentSimulationService.ProcessPaymentAsync()
   ?   ??> Get Fee + Payment from DB
   ?   ??> Check idempotency (already paid?)
   ?   ??> Update Payment.Status based on simulation result
   ?   ?   • Success ? Status = Paid, AmountPaid = Amount
   ?   ?   • Failed ? Status = Failed
   ?   ??> SaveChanges (database th?t!)
   ??> Redirect to PaymentResult.cshtml

7. User: PaymentResult.cshtml
   ??> Display payment result
   ??> If Success ? Link to RenterProfile
   ??> If Failed ? Link to Retry payment

8. User: Check booking status
   ??> Payment.Status = Paid ? Waiting for staff verification
```

---

## ?? Files Structure

### ? Created/Modified Files

| File | Purpose | Type |
|------|---------|------|
| `WebApp/Services/PaymentSimulationService.cs` | Simulate VNPay + update DB | **NEW** |
| `WebApp/Areas/Renter/Pages/Booking/Confirm.cshtml.cs` | Call MediatR directly | **MODIFIED** |
| `WebApp/Areas/Renter/Pages/Booking/ProcessPayment.cshtml.cs` | Create simulated payment URL | **MODIFIED** |
| `WebApp/Areas/Renter/Pages/Booking/PaymentGatewaySimulator.cshtml` | Simulated VNPay gateway UI | **NEW** |
| `WebApp/Areas/Renter/Pages/Booking/PaymentGatewaySimulator.cshtml.cs` | Process simulated payment | **NEW** |
| `WebApp/Areas/Renter/Pages/Booking/PaymentResult.cshtml` | Display result | **MODIFIED** |
| `WebApp/Areas/Renter/Pages/Booking/PaymentResult.cshtml.cs` | Receive result parameters | **MODIFIED** |
| `WebApp/Program.cs` | Register PaymentSimulationService | **MODIFIED** |

### ? Deleted Files (No longer needed)

| File | Reason |
|------|--------|
| `WebApp/Services/ApiClient.cs` | No API calls to WebAPI |
| `WebApp/Models/Api/PaymentModels.cs` | Not using API DTOs |

---

## ?? PaymentSimulationService Details

### Key Methods

#### 1. CreatePaymentUrlAsync
```csharp
public async Task<SimulatedPaymentResponse> CreatePaymentUrlAsync(
    Guid bookingId,
    decimal amount,
    string description,
    CancellationToken ct = default)
```

**Function:**
- Validate booking exists
- Get fee and payment from database
- Validate amount matches
- Generate transaction ID (format: `SIMyyyyMMddHHmmss9999`)
- Return simulated payment URL

**Returns:**
```csharp
{
    "Success": true,
    "PaymentUrl": "/Renter/Booking/PaymentGatewaySimulator?bookingId=...&amount=...&txnRef=...",
    "TransactionId": "SIM202401011200001234",
    "BookingId": "guid-here",
    "Amount": 500000,
    "CreatedAt": "2024-01-01T12:00:00Z"
}
```

#### 2. ProcessPaymentAsync
```csharp
public async Task<PaymentResultDto> ProcessPaymentAsync(
    Guid bookingId,
    string transactionId,
    PaymentSimulationResult simulationResult,
    CancellationToken ct = default)
```

**Function:**
- Get fee and payment from database
- **Idempotency check**: If already paid, return success immediately
- Update Payment based on simulation result:
  - **Success**: `Status = Paid`, `AmountPaid = Amount`, `ProviderReference = TransactionId`
  - **Failed**: `Status = Failed`, `ProviderReference = FAILED_{TransactionId}`
- **SaveChanges** (updates real database!)
- Return PaymentResultDto

**Returns:**
```csharp
{
    "Success": true,
    "BookingId": "guid-here",
    "TransactionId": "SIM202401011200001234",
    "Amount": 500000,
    "PaymentDate": "2024-01-01T12:00:00Z",
    "Message": "Payment successful"
}
```

### Payment Simulation Results

```csharp
public enum PaymentSimulationResult
{
    Success,              // ? Payment successful
    InsufficientBalance,  // ? Not enough balance
    InvalidCard,          // ? Invalid card details
    Timeout,              // ?? Transaction timeout
    UserCancelled         // ?? User cancelled
}
```

---

## ?? Testing Flow

### Step 1: Start WebApp

```bash
cd WebApp
dotnet run

# Navigate to: https://localhost:5001
```

### Step 2: Test Happy Path (Success)

1. **Login** as Renter
2. **Create Booking**:
   - Navigate to `/Renter/Booking/Create`
   - Select vehicle + time range
   - Click "Ti?p Theo"
3. **Payment**:
   - Payment method: VNPay QR (pre-selected)
   - Deposit: 20% of estimated cost
   - Click "Ti?p Theo"
4. **Confirm**:
   - Review information
   - Check "?i?u kho?n d?ch v?"
   - Click "Xác Nh?n ??t Ch?"
5. **ProcessPayment**:
   - Auto-redirects to PaymentGatewaySimulator
6. **PaymentGatewaySimulator**:
   - **Select**: "Thành Công" (Success) ?
   - Click "Xác Nh?n Thanh Toán"
   - Simulates 2-second delay
7. **PaymentResult**:
   - Displays success message
   - Shows transaction details
   - Link to "Xem ??t Ch? C?a Tôi"

### Step 3: Verify Database

```sql
SELECT 
    b.BookingId,
    b.Status AS BookingStatus,
    b.VerificationStatus,
    f.Amount AS DepositAmount,
    p.Status AS PaymentStatus,
    p.AmountPaid,
    p.ProviderReference,
    p.Method
FROM Bookings b
JOIN Fees f ON b.BookingId = f.BookingId
JOIN Payments p ON f.FeeId = p.FeeId
WHERE b.RenterId = 'YOUR_RENTER_ID'
ORDER BY b.CreatedAt DESC;
```

**Expected Results:**
```
BookingStatus: Pending_Verification
VerificationStatus: Pending
PaymentStatus: Paid ?
AmountPaid: 500000
ProviderReference: SIM202401011200001234
Method: VNPay_QR
```

### Step 4: Test Failure Scenarios

Repeat steps 1-6, but in **PaymentGatewaySimulator**, select:

#### Scenario A: Insufficient Balance
- **Select**: "S? D? Không ??"
- **Result**: Payment.Status = Failed
- **Message**: "Insufficient balance"
- **Can Retry**: Yes, from PaymentResult page

#### Scenario B: Invalid Card
- **Select**: "Th? Không H?p L?"
- **Result**: Payment.Status = Failed
- **Message**: "Invalid card"

#### Scenario C: Timeout
- **Select**: "Timeout"
- **Result**: Payment.Status = Failed
- **Message**: "Payment timeout"

#### Scenario D: User Cancelled
- **Select**: "Ng??i Dùng H?y"
- **Result**: Payment.Status = Failed
- **Message**: "User cancelled payment"

---

## ?? Testing Scenarios

### ? Happy Path

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create booking + VNPay | Booking created, Payment.Status = Pending |
| 2 | Select "Success" in simulator | Payment.Status = Paid, AmountPaid = Amount |
| 3 | View result page | Shows success + transaction ID |
| 4 | Check booking in profile | Booking shows "Ch? Xác Minh" |

### ? Failure Scenarios

| Scenario | Simulation Result | Expected Payment.Status | Can Retry? |
|----------|-------------------|-------------------------|------------|
| Insufficient balance | InsufficientBalance | Failed | ? Yes |
| Invalid card | InvalidCard | Failed | ? Yes |
| Timeout | Timeout | Failed | ? Yes |
| User cancelled | UserCancelled | Failed | ? Yes |

### ?? Idempotency Test

1. **Create booking** ? Payment.Status = Pending
2. **Complete payment** (Success) ? Payment.Status = Paid
3. **Call ProcessPaymentAsync again** with same BookingId
4. **Expected**: Returns success immediately without updating (idempotent)
5. **Database**: Payment.Status remains Paid, no changes

### ?? Retry Payment Test

1. **Create booking** ? Payment.Status = Pending
2. **Complete payment** (Failed - InsufficientBalance)
3. **Click "Th? L?i"** on PaymentResult page
4. **Expected**: Redirects to ProcessPayment with same BookingId
5. **Select "Success"** in simulator
6. **Expected**: Payment.Status = Paid (retry successful)

---

## ?? Security Checklist

### ? Authentication & Authorization

- [x] All booking pages require `[Authorize(Roles = "Renter")]`
- [x] RenterId extracted from **token claims** (not from client input)
- [x] Payment pages accessible only to authenticated users

### ? Data Integrity

- [x] Payment.Status = **Pending** on creation (not Paid)
- [x] Only **PaymentSimulationService** can update Payment.Status
- [x] **Idempotent** payment processing (duplicate calls handled)
- [x] Amount validation (must match Fee.Amount)

### ? Business Rules

- [x] Booking created with **Pending_Verification** status
- [x] Payment must be **Paid** before staff can approve booking
- [x] Cancel only allowed from **Pending_Verification** status
- [x] **95% refund** on cancellation (5% transaction fee)

---

## ?? Monitoring & Logging

### Key Log Points

```csharp
// PaymentSimulationService
_logger.LogInformation("[PAYMENT SIM] Creating payment URL for booking {BookingId}, amount {Amount}", ...);
_logger.LogInformation("[PAYMENT SIM] Processing payment for booking {BookingId}, result: {Result}", ...);
_logger.LogInformation("[PAYMENT SIM] Payment successful for booking {BookingId}, transaction: {TransactionId}", ...);
_logger.LogWarning("[PAYMENT SIM] Payment failed for booking {BookingId}", ...);

// Confirm.cshtml.cs
_logger.LogInformation("Booking created successfully: {BookingId}", bookingId);

// PaymentGatewaySimulator.cshtml.cs
_logger.LogInformation("Processing simulated payment for booking {BookingId} with result {Result}", ...);

// PaymentResult.cshtml.cs
_logger.LogInformation("Payment successful for booking {BookingId}, transaction: {TransactionId}", ...);
_logger.LogWarning("Payment failed for booking {BookingId}, message: {Message}", ...);
```

### Metrics to Track

- Total bookings created
- Payment success rate (by simulation result)
- Payment failure rate (by error type)
- Average time from booking to payment completion
- Retry payment rate
- Idempotent call rate

---

## ?? Troubleshooting

### Problem: "Payment not found for booking"

**Symptoms:**
- ProcessPayment shows error
- Cannot create payment URL

**Diagnosis:**
```sql
SELECT b.BookingId, f.FeeId, p.PaymentId, p.Status
FROM Bookings b
LEFT JOIN Fees f ON b.BookingId = f.BookingId AND f.Type = 'Deposit'
LEFT JOIN Payments p ON f.FeeId = p.FeeId
WHERE b.BookingId = 'BOOKING_ID';
```

**Solutions:**
1. Verify booking was created successfully
2. Check CreateBookingCommand created Fee and Payment
3. Verify database transaction committed
4. Check logs for CreateBookingCommand errors

### Problem: Payment status not updating

**Symptoms:**
- Simulator shows success
- But Payment.Status still Pending in database

**Diagnosis:**
```csharp
// Check logs for:
"[PAYMENT SIM] Processing payment for booking {BookingId}, result: {Result}"
"[PAYMENT SIM] Payment successful for booking {BookingId}, transaction: {TransactionId}"
```

**Solutions:**
1. Verify PaymentSimulationService is registered in DI
2. Check database connection string
3. Verify SaveChangesAsync is called
4. Check for database locks/timeouts
5. Verify transaction is not rolled back

### Problem: Idempotency not working

**Symptoms:**
- Payment updated multiple times
- AmountPaid changes on retry

**Diagnosis:**
```csharp
// Check idempotency check in ProcessPaymentAsync:
if (payment.Status == PaymentStatus.Paid && !string.IsNullOrEmpty(payment.ProviderReference))
{
    // Should return immediately
}
```

**Solutions:**
1. Verify payment.Status is set to Paid on first success
2. Verify payment.ProviderReference is set
3. Check concurrency handling (RowVersion)
4. Add database-level unique constraint on ProviderReference

---

## ?? Comparison: Standalone vs API-based

| Feature | Standalone (WebApp) | API-based (WebApp + WebAPI) |
|---------|---------------------|------------------------------|
| **Architecture** | Monolith (Razor Pages + MediatR) | Distributed (Frontend + Backend) |
| **Payment Gateway** | Simulated (PaymentSimulationService) | Real VNPay API integration |
| **Database Update** | Direct (via MediatR) | Via API calls |
| **Deployment** | Single app | 2 separate apps |
| **Best For** | **Sandbox/Demo** ? | **Production** ? |
| **Complexity** | Low | Medium |
| **Scalability** | Limited | High |
| **IPN Webhook** | Not needed (internal) | Required (public URL) |
| **Testing** | Easy (local only) | Complex (needs ngrok) |

---

## ?? Next Steps

### Phase 1: Complete Basic Flow ?
- [x] Create booking with payment pending
- [x] Simulate payment gateway
- [x] Update payment status in database
- [x] Display payment result
- [x] Support retry on failure

### Phase 2: Enhanced Features
- [ ] Add retry payment from booking list (RenterProfile)
- [ ] Add payment history view
- [ ] Add transaction logs table
- [ ] Add email notification after payment (simulated)
- [ ] Add payment receipt download (PDF)

### Phase 3: Production Migration
- [ ] Migrate to real VNPay API (WebAPI project)
- [ ] Replace PaymentSimulationService with VNPay SDK
- [ ] Configure IPN webhook (public URL)
- [ ] Add payment reconciliation job
- [ ] Add rate limiting for payment creation
- [ ] Add audit logging for payment status changes

---

## ?? Summary Checklist

Before using in production:

### WebApp
- [x] PaymentSimulationService registered in DI
- [x] MediatR configured correctly
- [x] Authentication/authorization working
- [x] TempData provider configured
- [x] Error handling for all pages
- [x] Loading states for async operations

### Database
- [x] Payment.Status has Pending enum value
- [x] Payment entity allows nullable PaidAt
- [x] Proper indexes on ForeignKeys
- [x] Transaction boundaries in CreateBookingCommand

### Testing
- [x] E2E test: Create ? Pay (Success)
- [x] E2E test: Create ? Pay (Failed) ? Retry
- [x] Idempotency test: Duplicate payment calls
- [ ] Load test: Concurrent bookings
- [ ] Security test: Authorization checks

### Migration to Production
- [ ] Replace PaymentSimulationService with real VNPay integration
- [ ] Deploy WebAPI separately
- [ ] Configure public IPN URL
- [ ] Switch to production VNPay credentials
- [ ] Add monitoring and alerting

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**WebApp**: Razor Pages (.NET 9) - Standalone  
**Payment**: Simulated (PaymentSimulationService)  
**Database**: Shared with WebAPI (optional)
