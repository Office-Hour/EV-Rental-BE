using Domain.Entities.StationManagement;
using Domain.Enums;

namespace Domain.Entities.BookingManagement;

public class Booking
{
    public Guid BookingId { get; set; }
    public Guid RenterId { get; set; }
    public Guid VehicleAtStationId { get; set; }
    public DateTime BookingCreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending_Verification;
    public BookingVerificationStatus VerificationStatus { get; set; } = BookingVerificationStatus.Pending;
    public Guid? VerifiedByStaffId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? CancelReason { get; set; }


    public virtual Renter Renter { get; set; } = null!;
    public virtual VehicleAtStation VehicleAtStation { get; set; } = null!;
    public virtual Staff? VerifiedByStaff { get; set; } // Navigation property for the staff who verified the booking

    /// <summary>
    /// Navigation property for related Fee entities.
    /// A booking always has at least one associated fee (deposit fee).
    /// </summary>
    public virtual ICollection<Fee> Fees { get; set; } = new List<Fee>();
}