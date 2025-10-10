using Domain.Enums;

namespace Application.DTOs.BookingManagement;

public class KycDto
{
    public Guid KycId { get; set; } = default;
    public Guid RenterId { get; set; } = default;
    public KycType Type { get; set; } = KycType.National_ID; // Default to National ID
    public string DocumentNumber { get; set; } = null!;
    public DateTime? ExpiryDate { get; set; }
    public KycStatus Status { get; set; } = KycStatus.Submitted; // Default to Submitted. Other statuses: Verified, Rejected
    public DateTime SubmittedAt { get; set; } = default;
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByStaffId { get; set; }
    public string? RejectionReason { get; set; }
}