using Domain.Entities.Abstract;

namespace Domain.Entities.Implement.Aggregates.Identity_KyC;

public class VerificationAudit : BaseEntity
{
    public Guid RenterId { get; set; }
    /// <summary>
    /// Staff Id that review the KYC
    /// </summary>
    public Guid ReviewerId { get; set; }
    public bool IsVerified { get; set; } = false;
    /// <summary>
    /// Reasons why verify/reject
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    public virtual Renter Renter { get; set; } = null!;
    public virtual DomainUser Reviewer { get; set; } = null!;
}
