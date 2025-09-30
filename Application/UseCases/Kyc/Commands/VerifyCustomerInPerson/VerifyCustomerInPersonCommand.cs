using MediatR;

namespace Application.UseCases.Kyc.Commands.VerifyCustomerInPerson;

public class VerifyCustomerInPersonCommand : IRequest<bool>
{
    public Guid RenterId { get; set; }
    public bool IsVerified { get; set; }
    public Guid VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
}
