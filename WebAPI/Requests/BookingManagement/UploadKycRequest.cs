using Domain.Enums;
using FastEndpoints;

namespace WebAPI.Requests.BookingManagement;

public class UploadKycRequest
{
    public KycType Type { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
}
