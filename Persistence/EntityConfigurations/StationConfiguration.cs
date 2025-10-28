using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;
/* =========================
   Admin
========================= */
public sealed class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> b)
    {
        b.ToTable("admins");

        b.HasKey(x => x.AdminId);
        b.Property(x => x.AdminId).ValueGeneratedNever();

        b.Property(x => x.Title).HasMaxLength(100);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.HireDate).HasDefaultValueSql("SYSUTCDATETIME()");

        b.Property(x => x.UserId).IsRequired();
        b.HasIndex(x => x.UserId).IsUnique();

        // If you have an Identity user class, uncomment:
        // b.HasOne<ApplicationUser>()
        //  .WithMany()
        //  .HasForeignKey(x => x.UserId)
        //  .OnDelete(DeleteBehavior.Restrict);

        // Avoid ambiguous inverse navigations (created vs approved)
        b.Ignore(x => x.VehicleTransfersHandled);
        b.Ignore(x => x.StaffTransfersHandled);
    }
}

/* =========================
   Pricing
========================= */
public sealed class PricingConfiguration : IEntityTypeConfiguration<Pricing>
{
    public void Configure(EntityTypeBuilder<Pricing> b)
    {
        b.ToTable("pricings");

        b.HasKey(x => x.PricingId);
        b.Property(x => x.PricingId).ValueGeneratedNever();

        b.Property(x => x.PricePerHour).HasColumnType("decimal(18,2)");
        b.Property(x => x.PricePerDay).HasColumnType("decimal(18,2)");
        b.Property(x => x.EffectiveFrom).IsRequired();
        b.Property(x => x.EffectiveTo);

        b.HasOne(x => x.Vehicle)
         .WithMany(v => v.Pricings)
         .HasForeignKey(x => x.VehicleId)
         .OnDelete(DeleteBehavior.Cascade);

        // Time-versioned per vehicle
        b.HasIndex(x => new { x.VehicleId, x.EffectiveFrom }).IsUnique();
    }
}

/* =========================
   StaffAtStation (history)
========================= */
public sealed class StaffAtStationConfiguration : IEntityTypeConfiguration<StaffAtStation>
{
    public void Configure(EntityTypeBuilder<StaffAtStation> b)
    {
        b.ToTable("staff_at_stations");

        b.HasKey(x => new { x.StaffId, x.StartTime });
        b.Property(x => x.StartTime).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.RoleAtStation).HasMaxLength(100);

        b.HasOne(x => x.Staff)
         .WithMany(s => s.StaffAtStations)
         .HasForeignKey(x => x.StaffId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Station)
         .WithMany(s => s.StaffAtStations)
         .HasForeignKey(x => x.StationId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.StaffId, x.EndTime });
        b.HasIndex(x => new { x.StationId, x.EndTime });

    }
}

/* =========================
   StaffTransfer
========================= */
public sealed class StaffTransferConfiguration : IEntityTypeConfiguration<StaffTransfer>
{
    public void Configure(EntityTypeBuilder<StaffTransfer> b)
    {
        b.ToTable("staff_transfers");

        b.HasKey(x => x.StaffTransferId);
        b.Property(x => x.StaffTransferId).ValueGeneratedNever();

        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.EffectiveFrom).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(TransferStatus.Draft);

        b.HasOne(x => x.Staff)
         .WithMany(s => s.StaffTransfers)
         .HasForeignKey(x => x.StaffId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.FromStation)
         .WithMany()
         .HasForeignKey(x => x.FromStationId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ToStation)
         .WithMany()
         .HasForeignKey(x => x.ToStationId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.CreatedByAdmin)
         .WithMany() // or WithMany(a => a.StaffTransfersCreated) if you split Admin navs
         .HasForeignKey(x => x.CreatedByAdminId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ApprovedByAdmin)
         .WithMany() // or WithMany(a => a.StaffTransfersApproved)
         .HasForeignKey(x => x.ApprovedByAdminId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.StaffId, x.Status });
        b.HasIndex(x => x.ToStationId);
        b.HasIndex(x => x.FromStationId);
        b.HasIndex(x => x.CreatedByAdminId);
        b.HasIndex(x => x.ApprovedByAdminId);
        b.HasIndex(x => x.EffectiveFrom);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_StaffTransfers_FromNotEqualTo",
            "[FromStationId] <> [ToStationId]"
        ));
    }
}

/* =========================
   Station
========================= */
public sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> b)
    {
        b.ToTable("stations");

        b.HasKey(x => x.StationId);
        b.Property(x => x.StationId).ValueGeneratedNever();

        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500).IsRequired();

        // Optional geo index later if needed
        // b.HasIndex(x => new { x.Latitude, x.Longitude });
    }
}

/* =========================
   Vehicle
========================= */
public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> b)
    {
        b.ToTable("vehicles");

        b.HasKey(x => x.VehicleId);
        b.Property(x => x.VehicleId).ValueGeneratedNever();

        b.Property(x => x.Make).HasMaxLength(100).IsRequired();
        b.Property(x => x.Model).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModelYear).IsRequired();

        // Keep as double (SQL float). If you prefer precision, uncomment:
        // b.Property(x => x.BatteryCapacityKwh).HasColumnType("decimal(10,2)");
        // b.Property(x => x.RangeKm).HasColumnType("decimal(10,2)");

        // Collections are configured on child entities
    }
}

/* =========================
   VehicleAtStation (history)
========================= */
public sealed class VehicleAtStationConfiguration : IEntityTypeConfiguration<VehicleAtStation>
{
    public void Configure(EntityTypeBuilder<VehicleAtStation> b)
    {
        b.ToTable("vehicle_at_stations");

        b.HasKey(x => x.VehicleAtStationId);
        b.Property(x => x.VehicleAtStationId).ValueGeneratedNever();

        b.Property(x => x.StartTime).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.CurrentBatteryCapacityKwh); // double
        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(VehicleAtStationStatus.Available);

        b.HasOne(x => x.Vehicle)
         .WithMany(v => v.VehicleAtStations)
         .HasForeignKey(x => x.VehicleId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Station)
         .WithMany(s => s.VehicleAtStations)
         .HasForeignKey(x => x.StationId)
         .OnDelete(DeleteBehavior.Cascade);

        // History indexes
        b.HasIndex(x => new { x.VehicleId, x.StartTime }).IsUnique();
        b.HasIndex(x => new { x.VehicleId, x.EndTime });
        b.HasIndex(x => new { x.StationId, x.EndTime });
    }
}

/* =========================
   VehicleTransfer
========================= */
public sealed class VehicleTransferConfiguration : IEntityTypeConfiguration<VehicleTransfer>
{
    public void Configure(EntityTypeBuilder<VehicleTransfer> b)
    {
        b.ToTable("vehicle_transfers");

        b.HasKey(x => x.VehicleTransferId);
        b.Property(x => x.VehicleTransferId).ValueGeneratedNever();

        b.Property(x => x.PickupNotes).HasMaxLength(1000);
        b.Property(x => x.DropoffNotes).HasMaxLength(1000);
        b.Property(x => x.Notes).HasMaxLength(1000);

        b.Property(x => x.Status).AsStringEnum().HasDefaultValue(TransferStatus.Draft);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        b.HasOne(x => x.Vehicle)
         .WithMany()
         .HasForeignKey(x => x.VehicleId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.FromStation)
         .WithMany()
         .HasForeignKey(x => x.FromStationId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ToStation)
         .WithMany()
         .HasForeignKey(x => x.ToStationId)
         .OnDelete(DeleteBehavior.Restrict);

        // Staff actions (optional)
        b.HasOne(x => x.PickedUpByStaff)
         .WithMany() // avoid ambiguous inverse on Staff
         .HasForeignKey(x => x.PickedUpByStaffId)
         .OnDelete(DeleteBehavior.NoAction);// handle set null on delete in application code

        b.HasOne(x => x.DroppedOffByStaff)
         .WithMany()
         .HasForeignKey(x => x.DroppedOffByStaffId)
         .OnDelete(DeleteBehavior.NoAction);// handle set null on delete in application code

        // Admins
        b.HasOne(x => x.CreatedByAdmin)
         .WithMany() // or WithMany(a => a.VehicleTransfersCreated) if you split Admin navs
         .HasForeignKey(x => x.CreatedByAdminId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ApprovedByAdmin)
         .WithMany() // or WithMany(a => a.VehicleTransfersApproved)
         .HasForeignKey(x => x.ApprovedByAdminId)
         .OnDelete(DeleteBehavior.Restrict);

        // Indexes / constraints
        b.HasIndex(x => x.Status);
        b.HasIndex(x => new { x.FromStationId, x.ToStationId });
        b.HasIndex(x => x.VehicleId);
        b.HasIndex(x => x.CreatedByAdminId);
        b.HasIndex(x => x.ApprovedByAdminId);
        b.HasIndex(x => x.ScheduledPickupAt);
        b.HasIndex(x => x.ScheduledDropoffAt);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_VehicleTransfers_FromNotEqualTo",
            "[FromStationId] <> [ToStationId]"
        ));
    }
}