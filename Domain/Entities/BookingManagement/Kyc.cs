using Domain.Enums;

namespace Domain.Entities.BookingManagement;

public class Kyc
{
    public Guid KycId { get; set; }
    public Guid RenterId { get; set; }
    public KycType Type { get; set; } = KycType.National_ID; // Default to National ID
    public string DocumentNumber { get; set; } = null!;
    public DateTime? ExpiryDate { get; set; }
    public KycStatus Status { get; set; } = KycStatus.Submitted; // Default to Submitted. Other statuses: Verified, Rejected
    public DateTime SubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByStaffId { get; set; }
    public string? RejectionReason { get; set; }


    public virtual Renter Renter { get; set; } = null!;
    /// <summary>
    /// Navigation property for the staff who verified the KYC document.
    /// </summary>
    public virtual Staff? VerifiedByStaff { get; set; }
}