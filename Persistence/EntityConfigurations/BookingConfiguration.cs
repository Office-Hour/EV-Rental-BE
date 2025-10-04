using Domain.Entities.BookingManagement;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.HasKey(x => x.BookingId);
        b.Property(x => x.BookingId).ValueGeneratedNever();

        b.Property(x => x.BookingCreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.StartTime).IsRequired();
        b.Property(x => x.EndTime).IsRequired();
        b.Property(x => x.CancelReason).HasMaxLength(1000);

        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(BookingStatus.Pending_Verification);
        b.Property(x => x.VerificationStatus).AsStringEnum().HasDefaultValue(BookingVerificationStatus.Pending);

        // Renter (1..*) ↔ Booking (*)
        b.HasOne(x => x.Renter)
         .WithMany(r => r.Bookings)
         .HasForeignKey(x => x.RenterId)
         .OnDelete(DeleteBehavior.Restrict);

        // VehicleAtStation (1..*) ↔ Booking (*)
        b.HasOne(x => x.VehicleAtStation)
         .WithMany(v => v.Bookings) // ensure VehicleAtStation has ICollection<Booking> Bookings
         .HasForeignKey(x => x.VehicleAtStationId)
         .OnDelete(DeleteBehavior.Restrict);

        // Verified by Staff (optional)
        b.HasOne(x => x.VerifiedByStaff)
         .WithMany(s => s.VerifiedBookings)
         .HasForeignKey(x => x.VerifiedByStaffId)
         .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        b.HasIndex(x => x.RenterId);
        b.HasIndex(x => x.VehicleAtStationId);
        b.HasIndex(x => x.VerifiedByStaffId);
        b.HasIndex(x => new { x.StartTime, x.EndTime }); // schedule search
    }
}

/* =========================
   Fee
========================= */
public class FeeConfiguration : IEntityTypeConfiguration<Fee>
{
    public void Configure(EntityTypeBuilder<Fee> b)
    {
        b.HasKey(x => x.FeeId);
        b.Property(x => x.FeeId).ValueGeneratedNever();

        b.Property(x => x.Type).AsStringEnum().HasDefaultValue(FeeType.Deposit);
        b.Property(x => x.Description).HasMaxLength(200).IsRequired();
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).AsStringEnum().HasDefaultValue(Currency.VND);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Booking (1) ↔ Fees (*)
        b.HasOne(x => x.Booking)
         .WithMany(bk => bk.Fees)
         .HasForeignKey(x => x.BookingId)
         .OnDelete(DeleteBehavior.Cascade);

        // One DEPOSIT per booking (filtered unique index)
        // Note: enum stored as string => 'Deposit'
        b.HasIndex(x => x.BookingId)
         .IsUnique()
         .HasFilter("[Type] = 'Deposit'");

        b.HasIndex(x => x.CreatedAt);
    }
}

/* =========================
   Payment
========================= */
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(x => x.PaymentId);
        b.Property(x => x.PaymentId).ValueGeneratedNever();

        b.Property(x => x.Method).AsStringEnum().HasDefaultValue(PaymentMethod.Unknown);
        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(PaymentStatus.Paid);
        b.Property(x => x.AmountPaid).HasColumnType("decimal(18,2)");
        b.Property(x => x.PaidAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.ProviderReference).HasMaxLength(200);

        // Enforce 1:1 Payment ↔ Fee by making FeeId a UNIQUE FK
        b.HasOne(x => x.Fee)
         .WithOne() // Fee has no Payment nav in your model
         .HasForeignKey<Payment>(x => x.FeeId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.FeeId).IsUnique();
        b.HasIndex(x => x.PaidAt);
    }
}

/* =========================
   Kyc
========================= */
public class KycConfiguration : IEntityTypeConfiguration<Kyc>
{
    public void Configure(EntityTypeBuilder<Kyc> b)
    {
        b.HasKey(x => x.KycId);
        b.Property(x => x.KycId).ValueGeneratedNever();

        b.Property(x => x.Type).AsStringEnum().HasDefaultValue(KycType.National_ID);
        b.Property(x => x.DocumentNumber).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(KycStatus.Submitted);
        b.Property(x => x.SubmittedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.RejectionReason).HasMaxLength(500);

        // Renter (1) ↔ Kyc (*)
        b.HasOne(x => x.Renter)
         .WithMany(r => r.Kycs)
         .HasForeignKey(x => x.RenterId)
         .OnDelete(DeleteBehavior.Cascade);

        // Verified by Staff (optional)
        b.HasOne(x => x.VerifiedByStaff)
         .WithMany() // no back-collection on Staff
         .HasForeignKey(x => x.VerifiedByStaffId)
         .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        b.HasIndex(x => x.RenterId);
        b.HasIndex(x => new { x.Type, x.Status });
        b.HasIndex(x => x.VerifiedByStaffId);
        b.HasIndex(x => x.SubmittedAt);
    }
}

/* =========================
   Renter
========================= */
public class RenterConfiguration : IEntityTypeConfiguration<Renter>
{
    public void Configure(EntityTypeBuilder<Renter> b)
    {
        b.HasKey(x => x.RenterId);
        b.Property(x => x.RenterId).ValueGeneratedNever();

        b.Property(x => x.DriverLicenseNo).HasMaxLength(64);
        b.Property(x => x.Address).HasMaxLength(500);
        b.Property(x => x.RiskScore).HasDefaultValue(0);

        // optional: ensure 1:1 User ↔ Renter via unique FK
        b.Property(x => x.UserId).IsRequired();
        b.HasIndex(x => x.UserId).IsUnique();
    }
}

/* =========================
   Staff
========================= */
public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> b)
    {
        b.HasKey(x => x.StaffId);
        b.Property(x => x.StaffId).ValueGeneratedNever();

        b.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.EmployeeCode).IsUnique();

        b.Property(x => x.Position).HasMaxLength(100).IsRequired();
        b.Property(x => x.HireDate).HasDefaultValueSql("SYSUTCDATETIME()");

        b.Property(x => x.UserId).IsRequired();
        b.HasIndex(x => x.UserId).IsUnique();
    }
}