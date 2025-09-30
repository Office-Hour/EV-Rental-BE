using MediatR;

namespace Application.UseCases.Kyc.Commands.VerifyCustomerInPerson;

public class VerifyCustomerInPersonCommandHandler : IRequestHandler<VerifyCustomerInPersonCommand, bool>
{
    public async Task<bool> Handle(VerifyCustomerInPersonCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
