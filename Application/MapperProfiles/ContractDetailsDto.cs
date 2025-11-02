using Domain.Enums;

namespace Application.MapperProfiles;

public class ContractDetailsDto
{
    public Guid ContractId { get; set; }
    public Guid RentalId { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Issued;
    public DateTime IssuedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public EsignProvider Provider { get; set; } = EsignProvider.Native;
    public string? ProviderEnvelopeId { get; set; }
    public string DocumentUrl { get; set; } = null!;
    public string DocumentHash { get; set; } = null!;
    public string? AuditTrailUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}