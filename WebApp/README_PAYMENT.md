# WebApp Standalone Payment System

## ?? Overview

**WebApp** là m?t **standalone Razor Pages application** v?i payment simulation system, **KHÔNG ph? thu?c vào WebAPI**. H? th?ng này s? d?ng:

- ? **MediatR** ?? g?i Commands/Queries tr?c ti?p
- ? **PaymentSimulationService** gi? l?p VNPay gateway
- ? **Real database updates** cho payment status
- ? **Clean Architecture** (Domain, Application, Persistence layers)

---

## ?? Quick Start

### 1. Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB ho?c SQL Server Express)
- Visual Studio 2022 / VS Code

### 2. Setup Database

```bash
# Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EVRentalDB;Trusted_Connection=True;"
}

# Run migrations (t? ??ng khi start app)
cd WebApp
dotnet run
```

### 3. Start Application

```bash
cd WebApp
dotnet run

# Navigate to: https://localhost:5001
```

### 4. Test Payment Flow

1. **Register/Login** as Renter
2. **Create Booking** (`/Renter/Booking/Create`)
3. **Select Payment Method** (`/Renter/Booking/Payment`)
4. **Confirm** (`/Renter/Booking/Confirm`)
5. **Simulated Gateway** (`/Renter/Booking/PaymentGatewaySimulator`)
   - Select payment result (Success/Failed/Timeout/etc.)
6. **View Result** (`/Renter/Booking/PaymentResult`)

---

## ?? Project Structure

```
WebApp/
??? Areas/
?   ??? Renter/
?       ??? Pages/
?           ??? Booking/
?               ??? Create.cshtml                    # Step 1: Select vehicle + time
?               ??? Payment.cshtml                   # Step 2: Payment method
?               ??? Confirm.cshtml                   # Step 3: Review + Create booking (MediatR)
?               ??? ProcessPayment.cshtml            # Step 4: Generate payment URL
?               ??? PaymentGatewaySimulator.cshtml   # Step 5: Simulated VNPay gateway
?               ??? PaymentResult.cshtml             # Step 6: Display result
??? Services/
?   ??? PaymentSimulationService.cs                  # ? Payment simulation + DB update
??? Program.cs                                       # DI registration
??? STANDALONE_PAYMENT_GUIDE.md                      # ?? Complete documentation
```

---

## ?? Key Components

### PaymentSimulationService

**Location:** `WebApp/Services/PaymentSimulationService.cs`

**Functions:**
```csharp
// 1. Create simulated payment URL
Task<SimulatedPaymentResponse> CreatePaymentUrlAsync(Guid bookingId, decimal amount, string description)

// 2. Process payment and update database
Task<PaymentResultDto> ProcessPaymentAsync(Guid bookingId, string transactionId, PaymentSimulationResult result)
```

**Features:**
- ? Validates booking/fee/payment
- ? Generates transaction ID (format: `SIMyyyyMMddHHmmss9999`)
- ? **Updates Payment.Status in real database**
- ? **Idempotent** (duplicate calls handled)
- ? Supports multiple test scenarios

### Payment Simulation Results

```csharp
public enum PaymentSimulationResult
{
    Success,              // ? Payment successful ? Status = Paid
    InsufficientBalance,  // ? Not enough balance ? Status = Failed
    InvalidCard,          // ? Invalid card ? Status = Failed
    Timeout,              // ?? Transaction timeout ? Status = Failed
    UserCancelled         // ?? User cancelled ? Status = Failed
}
```

---

## ?? Payment Flow Diagram

```mermaid
sequenceDiagram
    participant User
    participant Create as Create.cshtml
    participant Payment as Payment.cshtml
    participant Confirm as Confirm.cshtml
    participant MediatR
    participant DB as Database
    participant Process as ProcessPayment
    participant Gateway as PaymentGatewaySimulator
    participant Result as PaymentResult

    User->>Create: Select vehicle + time
    Create->>Payment: TempData[BookingData]
    
    User->>Payment: Select payment method
    Payment->>Confirm: TempData[PaymentData]
    
    User->>Confirm: Review + Submit
    Confirm->>MediatR: CreateBookingCommand
    MediatR->>DB: Create Booking (Pending)
    MediatR->>DB: Create Fee (Deposit)
    MediatR->>DB: Create Payment (Pending, AmountPaid=0)
    MediatR-->>Confirm: BookingId
    Confirm->>Process: Redirect with BookingId
    
    Process->>Gateway: Generate payment URL
    User->>Gateway: Select simulation result
    Gateway->>DB: Update Payment.Status
    Gateway->>Result: Redirect with result
    Result->>User: Display success/failure
```

---

## ?? Testing Scenarios

### ? Happy Path (Success)

```
Create Booking ? Select Payment ? Confirm
  ? Gateway Simulator (Select "Success")
  ? Payment.Status = Paid ?
  ? Display success message
```

### ? Failure Path (Insufficient Balance)

```
Create Booking ? Select Payment ? Confirm
  ? Gateway Simulator (Select "Insufficient Balance")
  ? Payment.Status = Failed ?
  ? Display error + "Retry" button
  ? Click Retry ? Back to Gateway
  ? Select "Success" ? Payment.Status = Paid ?
```

### ?? Idempotency Test

```
Create Booking ? Complete Payment (Success)
  ? Call ProcessPaymentAsync again
  ? Returns success immediately (no database update)
  ? Payment.Status remains Paid
```

---

## ?? Database Schema

### Payment Entity

```csharp
public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid FeeId { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending; // ? Default
    public decimal AmountPaid { get; set; }
    public DateTime? PaidAt { get; set; }  // ? Nullable
    public string? ProviderReference { get; set; }
    
    public virtual Fee Fee { get; set; }
}

public enum PaymentStatus
{
    Pending,  // ? Initial state
    Paid,     // After successful payment
    Refunded, // After refund
    Failed    // Payment failed
}
```

### Payment Lifecycle

```
1. CreateBooking ? Payment.Status = Pending, AmountPaid = 0
2. Gateway Success ? Payment.Status = Paid, AmountPaid = Amount
3. Gateway Failed ? Payment.Status = Failed
4. Retry Success ? Payment.Status = Paid
```

---

## ?? Security Features

### ? Implemented

- [x] **RenterId from token** (not from client input)
- [x] **Authorization** (`[Authorize(Roles = "Renter")]`)
- [x] **Amount validation** (must match Fee.Amount)
- [x] **Idempotent payment** processing
- [x] **Payment status transitions** enforced

### ?? Business Rules

- ? Booking created with `Status = Pending_Verification`
- ? Payment created with `Status = Pending`, `AmountPaid = 0`
- ? Only `PaymentSimulationService` can update Payment.Status
- ? Staff can only approve if `Payment.Status = Paid`
- ? Cancel allowed only from `Pending_Verification`
- ? **95% refund** on cancellation (5% transaction fee)

---

## ?? Documentation

| Document | Description |
|----------|-------------|
| [STANDALONE_PAYMENT_GUIDE.md](STANDALONE_PAYMENT_GUIDE.md) | Complete integration guide with testing scenarios |
| [PAYMENT_INTEGRATION_GUIDE.md](PAYMENT_INTEGRATION_GUIDE.md) | ?? Old guide for API-based approach (deprecated) |

---

## ?? Standalone vs API-based

| Feature | **Standalone (Current)** | API-based (WebAPI) |
|---------|--------------------------|---------------------|
| Architecture | Monolith | Distributed |
| Payment | **Simulated** ? | Real VNPay API |
| Database | Direct (MediatR) | Via API calls |
| Deployment | Single app | 2 apps |
| Best For | **Sandbox/Demo** ? | Production |
| Complexity | Low | Medium |
| Testing | Easy (local) | Complex (ngrok) |

---

## ?? Troubleshooting

### Payment status not updating?

```sql
-- Check Payment status
SELECT b.BookingId, p.Status, p.AmountPaid, p.ProviderReference
FROM Bookings b
JOIN Fees f ON b.BookingId = f.BookingId
JOIN Payments p ON f.FeeId = p.FeeId
WHERE b.BookingId = 'YOUR_BOOKING_ID';
```

### Idempotency not working?

```csharp
// Check logs for:
"[PAYMENT SIM] Payment already processed for booking {BookingId}"
```

### Cannot create booking?

```csharp
// Check logs for:
"Booking created successfully: {BookingId}"
```

---

## ?? Next Steps

### Phase 1: Demo/Sandbox ? (Current)
- [x] Standalone payment simulation
- [x] Database updates
- [x] Multiple test scenarios
- [x] Retry on failure

### Phase 2: Production Migration
- [ ] Integrate real VNPay API (in WebAPI project)
- [ ] Replace PaymentSimulationService with VNPay SDK
- [ ] Configure IPN webhook
- [ ] Deploy WebAPI separately
- [ ] Switch to production credentials

---

## ?? Support

For issues or questions:
1. Check [STANDALONE_PAYMENT_GUIDE.md](STANDALONE_PAYMENT_GUIDE.md)
2. Review logs in Output window
3. Check database with provided SQL queries
4. Verify PaymentSimulationService is registered in Program.cs

---

**Version:** 1.0  
**Framework:** .NET 9  
**Architecture:** Clean Architecture + MediatR  
**Payment:** Simulated (Standalone)  
**Status:** ? Production-ready for Sandbox/Demo
