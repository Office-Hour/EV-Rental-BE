using Domain.Entities.BookingManagement;
using Domain.Enums;

namespace Domain.Entities.StationManagement;

public class VehicleTransfer
{
    public Guid VehicleTransferId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    public Guid? PickedUpByStaffId { get; set; } // Null if not yet picked up
    public DateTime? PickedUpAt { get; set; } // Null if not yet picked up
    public Guid? DroppedOffByStaffId { get; set; } // Null if not yet dropped off
    public DateTime? DroppedOffAt { get; set; } // Null if not yet dropped off
    public string? PickupNotes { get; set; }
    public string? DropoffNotes { get; set; }
    public Guid? ApprovedByAdminId { get; set; } // Null if not yet approved
    public Guid CreatedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ScheduledPickupAt { get; set; }
    public DateTime? ScheduledDropoffAt { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Draft; // Default to Draft. Other values: Pending, InTransit, Completed, Cancelled
    public string? Notes { get; set; }


    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual Station FromStation { get; set; } = null!;
    public virtual Station ToStation { get; set; } = null!;
    /// <summary>
    /// Gets or sets the staff who picked up the vehicle for transfer.
    /// Handle set null on delete in application code
    /// </summary>
    public virtual Staff? PickedUpByStaff { get; set; }
    /// <summary>
    /// Gets or sets the staff who dropped off the vehicle at the destination station.
    /// Handle set null on delete in application code
    /// </summary>
    public virtual Staff? DroppedOffByStaff { get; set; }
    public virtual Admin? ApprovedByAdmin { get; set; }
    public virtual Admin CreatedByAdmin { get; set; } = null!;
}