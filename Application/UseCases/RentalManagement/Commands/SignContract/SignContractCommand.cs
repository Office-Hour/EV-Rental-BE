using Application.DTOs.RentalManagement;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.SignContract;

public class SignContractCommand : IRequest<Guid>
{
    public CreateSignaturePayloadDto CreateSignaturePayloadDto { get; set; } = null!;
    public ESignPayloadDto? ESignPayload { get; set; }
}
