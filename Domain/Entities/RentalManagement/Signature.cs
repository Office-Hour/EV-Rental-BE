using Domain.Enums;

namespace Domain.Entities.RentalManagement;

public class Signature
{
    public Guid SignatureId { get; set; }
    public Guid ContractId { get; set; }
    public PartyRole Role { get; set; } // renter/staff
    public SignatureEvent SignatureEvent { get; set; } // pickup/dropoff
    public SignatureType Type { get; set; } // esign/handwritten
    public DateTime SignedAt { get; set; }
    public string? SignerIp { get; set; }
    public string? UserAgent { get; set; }
    public string? ProviderSignatureId { get; set; }
    public string? SignatureImageUrl { get; set; }
    public string? CertSubject { get; set; }
    public string? CertIssuer { get; set; }
    public string? CertSerial { get; set; }
    public string? CertFingerprintSha256 { get; set; }
    public string? SignatureHash { get; set; }
    public string? EvidenceUrl { get; set; }


    public virtual Contract Contract { get; set; } = null!;
}
