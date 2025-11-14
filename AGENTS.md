# Copilot Review Instructions — C# Clean Architecture + CQRS/MediatR (EV Rental Booking)

> Paste this file at the root of your repo or as a PR top comment. At the end of this doc, paste the specific files/snippets you want Copilot to read.

---

## 🎯 Role & Scope

**Role:** Act as a **Senior C# Developer & Software Architect** reviewing a Clean Architecture codebase that uses **CQRS with MediatR** and **EF Core**.

**Scope:** Review the specified **Commands/Queries & Handlers** (Application layer), related **Domain Entities** (Domain layer), and **EntityTypeConfiguration** classes (Persistence layer) they depend on.

**Goal:** Identify and fix (a) **runtime defects**, (b) **business‑logic errors**, and (c) **data‑model/config issues** that could cause correctness, security, or consistency problems in the **EV Rental booking flow**.

---

## 🧭 Business Context & Flow (Source of Truth)

Validate the code and configs against this intended process:

1. User registers an account with role `Renter`.
2. User logs in → receives `accessToken` + `refreshToken`.
3. User fetches renter profile and sees their `RenterId`.
4. User uploads **KYC**.
5. User can **create a Booking** only after **deposit payment** succeeds; system stores **payment + fee** for later refund.
6. If Booking is still `PendingVerification`, user may **request cancel** ⇒ receives **cancel code** via Email/SMS ⇒ cancels using this code; **refund = 95%** of deposit (**5% transaction fee**).
7. At the station, **Staff** checks in the booking and sets `BookingVerificationStatus = Approved` after validating KYC vs. ID/passport (or rejects).
8. Staff **creates Rental** for the **Approved** booking.
9. Staff **creates Contract** for the Rental.
10. Staff **creates Inspection** for the vehicle at renter check‑in (pre‑rental condition).
11. **Renter + Staff sign** the contract.
12. **Renter sets Rental status to `In_Progress`**.

### State/Transition Rules to Enforce

- **Booking**: `PendingVerification → Approved → (RentalCreated)`.

  - **Cancellation** allowed **only** from `PendingVerification` with valid code & TTL.

- **Payment**: deposit must be **captured/confirmed** before booking creation is finalized.

  - On cancel, refund **0.95 × deposit** (define rounding rules).

- **Rental**: can be created **only when Booking is `Approved`** and not already linked to an **active** Rental.
- **Contract**: requires existing Rental; **signing** requires completed **pre‑rental Inspection**.
- After signatures, Rental may transition to **`In_Progress`** (no duplicate transitions).

---

## 📂 Files to Analyze (Paste Below)

> Paste paths/snippets for:
>
> - **Commands/Queries & Handlers** (Application, MediatR)
> - **Domain Entities** (Domain)
> - **EntityTypeConfiguration** (Persistence)
> - Supporting services (Payment, Email/SMS, KYC, Auth)

---

## ✅ Deliverable Format (Strict)

Produce a **structured review** with these sections:

1. **Blocking Runtime Defects**

   - Each finding: **Title**, **Evidence (file:line)**, **Why it breaks at runtime**, **Minimal Fix (diff or snippet)**.

2. **Business Logic Errors**

   - Map each finding to a violated rule from **Business Context** above.
   - Include **Title**, **Evidence**, **Impact**, **Fix**, and **Guard Rails** (extra checks/tests).

3. **Data Model & EF Core Mapping Issues**

   - Keys/relationships, `DeleteBehavior`, required fields, indexes, enum mapping, precision.
   - For each: **Title**, **Evidence**, **Impact**, **Fix (EntityTypeConfiguration snippet)**.

4. **Security & Consistency Checks**

   - AuthZ/roles in handlers (`Renter` vs `Staff`), token verification, ID spoofing prevention,
     idempotency (payments/cancel), transaction boundaries, concurrency.
   - For each: **Title**, **Evidence**, **Impact**, **Fix**.

5. **Test Gaps (Add These Tests)**

   - List concrete unit/integration tests with Given/When/Then.
   - Include **one test stub per blocking finding** (xUnit/NUnit snippet).

6. **Summary Checklist — Pass/Fail**

   - Bullet verdict per step of the flow; mark **✅/❌** with 1‑line rationale.

---

## 🔍 Checklist the Review Must Cover (Exhaustive)

### A. MediatR & CQRS

- Commands vs. Queries separation respected; **no writes** in Queries, no heavy reads in Commands.
- Handlers are **async**, accept `CancellationToken`, and pass it down to EF calls.
- Validation pipeline (FluentValidation or similar) for required inputs.
- **Idempotency**: commands that can be retried (payments, cancel with code, status transitions) are idempotent.

### B. Domain & Invariants

- **Status transitions** enforced (compile a small transition table per entity). Reject invalid transitions with domain exceptions.
- Single source of truth for **deposit amount**, **fee percent (5%)**, and **refund** calculation (currency‑safe).
- Prevent **double rental creation** for the same booking.
- **Cancel code**: uniqueness, TTL, one‑time use, renter‑scoped, rate limiting.

### C. EF Core Mapping & Data Integrity

- Required properties marked `.IsRequired()`.
- **Money fields** use `decimal(18,2)` (or appropriate precision) and consistent rounding (banker’s or away‑from‑zero — specify).
- Use **`DateTimeOffset`** for timestamps; store **UTC**.
- Enums mapped as strings with `HasConversion<string>()` when appropriate.
- Relationships & `DeleteBehavior`: avoid accidental cascade deleting Payments when a Booking is removed; prefer `Restrict`/`NoAction` where appropriate.
- Concurrency: add **RowVersion** (`byte[]` with `.IsRowVersion().IsConcurrencyToken()`) to critical aggregates (`Booking`, `Rental`, `Contract`, `Payment`).
- Unique indexes: enforce uniqueness for cancel codes, one active rental per booking, one active verification per booking, etc.
- Query performance: indexes on FKs and common filters (`Booking.Status`, `Booking.RenterId`, `Payment.BookingId`, `Rental.BookingId`).

### D. Transactions & Consistency

- Multi‑write operations (booking + payment + fee) wrapped in a **single transaction** with `ExecutionStrategy` retry.
- **Outbox/domain events** for cross‑aggregate consistency (e.g., send Email/SMS cancel code **after commit**).
- Payment webhooks/callbacks are **idempotent** (check existing payment status before updating).

### E. Security & Authorization

- Handlers verify `UserId` and `Role` (`Renter`, `Staff`) explicitly; do **not** trust client‑provided IDs.
- KYC objects access‑controlled; only staff can approve; renter can only view own KYC.
- Cancel flow: renter must **own the booking**; code comparison time‑constant if feasible; throttle attempts.
- Tokens validated; do not mix identity from token with IDs in the body.

### F. API/Handler Quality

- Clear error types (problem details) and consistent HTTP → Application exception mapping.
- Null guards; defensive coding on optional navigations.
- `AsNoTracking()` for Queries; tracking only where mutation happens.
- Pagination on list queries; avoid N+1 (use `Include`/`ThenInclude` judiciously or projections).

### G. Logging/Observability

- Structured logs on state changes: booking approved, canceled, rental created, contract signed.
- Correlation IDs across request/handler/db transaction.
- Audit trail entities for critical changes (who/when/old/new).

---

## 🧩 Helpful Snippet Patterns (For Proposed Fixes)

**EF: money precision & enum as string**

```csharp
builder.Property(x => x.DepositAmount)
    .HasColumnType("decimal(18,2)")
    .IsRequired();

builder.Property(x => x.Status)
    .HasConversion<string>()
    .IsRequired();
```

**EF: rowversion for optimistic concurrency**

```csharp
builder.Property<byte[]>("RowVersion")
    .IsRowVersion()
    .IsConcurrencyToken();
```

**EF: delete behavior safety**

```csharp
builder.HasOne(p => p.Booking)
    .WithMany(b => b.Payments)
    .HasForeignKey(p => p.BookingId)
    .OnDelete(DeleteBehavior.Restrict);
```

**Handler: status transition guard + idempotency**

```csharp
if (booking.Status != BookingStatus.PendingVerification)
    return Result.Failure("Only PendingVerification bookings can be canceled.");

if (booking.IsCanceled)
    return Result.Success(); // idempotent

var refund = Math.Round(
    booking.DepositAmount * 0.95m,
    2,
    MidpointRounding.AwayFromZero // keep consistent across app
);
```

**Query: no tracking + pagination**

```csharp
var data = await _db.Bookings
    .AsNoTracking()
    .Where(b => b.RenterId == renterId)
    .OrderByDescending(b => b.CreatedAt)
    .Skip((page - 1) * size)
    .Take(size)
    .ToListAsync(ct);
```

---

## 🧪 Minimum Test Set to Add (Examples)

- **Cancel Pending Booking** → 95% refund, single‑use cancel code, code TTL enforced.
- **Prevent Double Rental** → second `CreateRentalCommand` for same booking rejected.
- **Approve Requires KYC** → booking cannot be approved without verified KYC.
- **Contract Requires Inspection** → signing blocked until inspection exists.
- **Idempotent Payment Callback** → duplicate webhook does not double‑apply payment.
- **Concurrency** → optimistic concurrency conflict on simultaneous updates handled.

> Provide **one test stub per blocking issue** you identify.

---

## ✅ Acceptance Criteria

- No unguarded invalid status transitions.
- Deposit & refund logic correct (95% refund; precise rounding; **single source of truth**).
- No path to create **Rental** before **Approval** or twice for the same Booking.
- Cancel code is **unique, expiring, single‑use, renter‑scoped**.
- EF Core mappings enforce integrity (required, FK, delete behaviors, concurrency, precision).
- All handlers are async with `CancellationToken` and properly **authorize roles**.
- Transactions wrap multi‑write flows; **outbox/event dispatch after commit**.
- Concrete **test stubs** included for each blocking issue.

---

## 📎 Paste Artifacts Here

- Commands/Queries & Handlers
- Domain Entities
- EntityTypeConfiguration
- Payment/KYC/Auth/Email-SMS services
