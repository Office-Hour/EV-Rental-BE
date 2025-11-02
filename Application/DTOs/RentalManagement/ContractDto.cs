using Domain.Enums;

namespace Application.DTOs.RentalManagement;

public class ContractDto
{
    public Guid ContractId { get; set; }
    public Guid RentalId { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Issued;
    public DateTime IssuedAt { get; set; }
}