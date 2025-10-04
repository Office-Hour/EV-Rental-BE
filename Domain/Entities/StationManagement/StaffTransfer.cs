using Domain.Entities.BookingManagement;
using Domain.Enums;

namespace Domain.Entities.StationManagement;
public class StaffTransfer
{
    public Guid StaffTransferId { get; set; }
    public Guid StaffId { get; set; }
    public Guid FromStationId { get; set; }
    public Guid ToStationId { get; set; }
    public Guid? ApprovedByAdminId { get; set; }
    public Guid CreatedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Draft;
    public string? Notes { get; set; }


    public virtual Staff Staff { get; set; } = null!;
    public virtual Station FromStation { get; set; } = null!;
    public virtual Station ToStation { get; set; } = null!;
    /// <summary>
    /// Gets or sets the admin who approved the staff transfer.
    /// If null, the transfer is pending approval.
    /// </summary>
    public virtual Admin? ApprovedByAdmin { get; set; }
    /// <summary>
    /// Gets or sets the admin who created the staff transfer.
    /// This field is mandatory and cannot be null.
    /// </summary>
    public virtual Admin CreatedByAdmin { get; set; } = null!;
}