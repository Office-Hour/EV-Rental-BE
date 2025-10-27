using Domain.Enums;

namespace WebAPI.Requests.RentalManagement;

public class CreateContractRequest
{
    public Guid RentalId { get; set; }
    public EsignProvider Provider { get; set; }
}
