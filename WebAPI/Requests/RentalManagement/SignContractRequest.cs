using Application.DTOs.RentalManagement;

namespace WebAPI.Requests.RentalManagement;

public class SignContractRequest
{
    public CreateSignaturePayloadDto CreateSignaturePayloadDto { get; set; } = null!;
    public ESignPayloadDto? ESignPayload { get; set; }
}
