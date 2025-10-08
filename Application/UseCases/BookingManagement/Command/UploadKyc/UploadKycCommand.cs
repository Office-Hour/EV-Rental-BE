using Domain.Enums;
using MediatR;

namespace Application.UseCases.BookingManagement.Command.UploadKyc;

public class UploadKycCommand : IRequest
{
    public Guid UserId { get; set; }
    public KycType Type { get; set; } // Default to National ID
    public string DocumentNumber { get; set; } = null!;
    public DateTime? ExpiryDate { get; set; } // Optional, as some documents may not have an expiry date
    public KycStatus Status { get; set; } = KycStatus.Submitted; // Default to Submitted. Other statuses: Verified, Rejected
}
