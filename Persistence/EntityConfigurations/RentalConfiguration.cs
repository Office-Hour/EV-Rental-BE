using Domain.Entities.BookingManagement;
using Domain.Entities.RentalManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public sealed class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> b)
    {
        b.ToTable("rentals");

        b.HasKey(x => x.RentalId);
        b.Property(x => x.RentalId).ValueGeneratedNever();

        b.Property(x => x.StartTime).IsRequired();
        b.Property(x => x.EndTime).IsRequired();
        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(RentalStatus.Reserved);

        b.Property(x => x.Score).IsRequired();
        b.Property(x => x.Comment).HasMaxLength(2000);
        b.Property(x => x.RatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // 1:1 with booking (enforced via unique index on BookingId)
        b.HasOne(r => r.Booking)
         .WithOne()                     // <-- tie to Booking.Rental nav
         .HasForeignKey<Rental>(r => r.BookingId)
         .OnDelete(DeleteBehavior.NoAction);// Handle cascade manually in app

        // Vehicle required
        b.HasOne<Vehicle>()
         .WithMany()                    // or .WithMany(v => v.Rentals) if you have the nav
         .HasForeignKey(x => x.VehicleId)
         .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        b.HasIndex(x => x.VehicleId);
        b.HasIndex(x => new { x.StartTime, x.EndTime });
        b.HasIndex(x => x.BookingId).IsUnique(); // one rental per booking

        // Score 1..5
        b.ToTable(t => t.HasCheckConstraint("CK_Rentals_Score_Range", "[Score] BETWEEN 1 AND 5"));
    }
}

/* =========================
   Contract
========================= */
public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("contracts");

        b.HasKey(x => x.ContractId);
        b.Property(x => x.ContractId).ValueGeneratedNever();

        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(ContractStatus.Issued);
        b.Property(x => x.IssuedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.CompletedAt);
        b.Property(x => x.Provider).AsStringEnum().HasDefaultValue(EsignProvider.Native);
        b.Property(x => x.ProviderEnvelopeId).HasMaxLength(256);

        b.Property(x => x.DocumentUrl).IsRequired().HasMaxLength(2048);
        b.Property(x => x.DocumentHash).IsRequired().HasMaxLength(128);
        b.Property(x => x.AuditTrailUrl).HasMaxLength(2048);
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasOne(x => x.Rental)
         .WithMany(x => x.Contracts)
         .HasForeignKey(x => x.RentalId)
         .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        b.HasIndex(x => x.RentalId);
        b.HasIndex(x => new { x.Provider, x.ProviderEnvelopeId });
        b.HasIndex(x => x.Status);
    }
}

/* =========================
   Inspection
========================= */
public sealed class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> b)
    {
        b.ToTable("inspections");

        b.HasKey(x => x.InspectionId);
        b.Property(x => x.InspectionId).ValueGeneratedNever();

        b.Property(x => x.Type).AsStringEnum().HasDefaultValue(InspectionType.Pre_Rental);
        b.Property(x => x.CurrentBatteryCapacityKwh).HasColumnType("decimal(18,2)");
        b.Property(x => x.InspectedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Required inspector: prevent cascading null; use Restrict
        b.HasOne(x => x.InspectorStaff)
         .WithMany(x => x.Inspections)
         .HasForeignKey(x => x.InspectorStaffId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Rental)
         .WithMany(x => x.Inspections)
         .HasForeignKey(x => x.RentalId)
         .OnDelete(DeleteBehavior.Cascade);

        // Exactly one pre and one post per rental
        b.HasIndex(x => new { x.RentalId, x.Type }).IsUnique();

        // Helpful indexes
        b.HasIndex(x => x.InspectorStaffId);
        b.HasIndex(x => x.InspectedAt);
    }
}

/* =========================
   Report
========================= */
public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> b)
    {
        b.ToTable("reports");

        b.HasKey(x => x.ReportId);
        b.Property(x => x.ReportId).ValueGeneratedNever();

        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.DamageFound).HasDefaultValue(false);

        b.HasOne(x => x.Inspection)
         .WithMany(x => x.Reports)
         .HasForeignKey(x => x.InspectionId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.InspectionId);
        b.HasIndex(x => x.DamageFound);
    }
}

/* =========================
   Signature
========================= */
public sealed class SignatureConfiguration : IEntityTypeConfiguration<Signature>
{
    public void Configure(EntityTypeBuilder<Signature> b)
    {
        b.ToTable("signatures");

        b.HasKey(x => x.SignatureId);
        b.Property(x => x.SignatureId).ValueGeneratedNever();

        b.Property(x => x.Role).AsStringEnum();
        b.Property(x => x.SignatureEvent).AsStringEnum();
        b.Property(x => x.Type).AsStringEnum();

        b.Property(x => x.SignedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.SignerIp).HasMaxLength(45);
        b.Property(x => x.UserAgent).HasMaxLength(512);
        b.Property(x => x.ProviderSignatureId).HasMaxLength(256);
        b.Property(x => x.SignatureImageUrl).HasMaxLength(2048);
        b.Property(x => x.CertSubject).HasMaxLength(512);
        b.Property(x => x.CertIssuer).HasMaxLength(512);
        b.Property(x => x.CertSerial).HasMaxLength(128);
        b.Property(x => x.CertFingerprintSha256).HasMaxLength(128);
        b.Property(x => x.SignatureHash).HasMaxLength(128);
        b.Property(x => x.EvidenceUrl).HasMaxLength(2048);

        b.HasOne(x => x.Contract)
         .WithMany(x => x.Signatures)
         .HasForeignKey(x => x.ContractId)
         .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        b.HasIndex(x => x.ContractId);
        b.HasIndex(x => x.SignedAt);
        b.HasIndex(x => x.ProviderSignatureId);
    }
}