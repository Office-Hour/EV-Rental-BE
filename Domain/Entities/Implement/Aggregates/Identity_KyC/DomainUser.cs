using Domain.Entities.Abstract;

namespace Domain.Entities.Implement.Aggregates.Identity_KyC;

public class DomainUser : BaseEntity
{
    public string IdentityUserId { get; set; } = null!;

    public Guid RenterId { get; set; }
    public virtual IEnumerable<VerificationAudit>? VerificationAudits { get; set; }
}
