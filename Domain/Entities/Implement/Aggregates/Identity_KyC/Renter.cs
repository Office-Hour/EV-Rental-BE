using Domain.Entities.Abstract;

namespace Domain.Entities.Implement.Aggregates.Identity_KyC;

public class Renter : BaseEntity
{
    public Guid DomainUserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = string.Empty;
    public DateTimeOffset? DOB { get; set; } = null;

    public virtual DomainUser User { get; set; } = null!;
    public virtual IEnumerable<Document> Documents { get; set; } = [];
    public virtual IEnumerable<VerificationAudit> VerificationAudits { get; set; } = [];
}
