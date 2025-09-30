using Domain.Entities.Abstract;
using Domain.Enums;

namespace Domain.Entities.Implement.Aggregates.Identity_KyC;

public class Document : BaseEntity
{
    public DocumentType DocumentType { get; set; } = DocumentType.None;
    
    public DocumentStatus DocumentStatus { get; set; } = DocumentStatus.Pending;
    public string Notes { get; set; } = string.Empty;

    public Guid? FrontImageId { get; set; }
    public virtual DomainBlob? FrontImage { get; set; }
    public Guid? BackImageId { get; set; }
    public virtual DomainBlob? BackImage { get; set; }
    public Guid RenterId { get; set; }
    public virtual Renter Renter { get; set; } = null!;
}
