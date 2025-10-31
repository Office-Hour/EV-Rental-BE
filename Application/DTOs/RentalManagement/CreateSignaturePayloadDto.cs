using Domain.Enums;

namespace Application.DTOs.RentalManagement;

public class CreateSignaturePayloadDto
{
    public Guid ContractId { get; set; }
    public string DocumentUrl { get; set; } = null!;
    public string DocumentHash { get; set; } = null!;
    public PartyRole Role { get; set; }
    public SignatureEvent SignatureEvent { get; set; } = SignatureEvent.Pickup;
    public SignatureType Type { get; set; } = SignatureType.OnPaper;
    public DateTime SignedAt { get; set; } = DateTime.UtcNow;
}