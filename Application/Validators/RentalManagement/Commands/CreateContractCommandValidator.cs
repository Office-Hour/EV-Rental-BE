using Application.UseCases.RentalManagement.Commands.CreateContract;
using FluentValidation;

namespace Application.Validators.RentalManagement.Commands;

public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(x => x.RentalId)
            .NotEmpty().WithMessage("RentalId is required.");
        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Invalid EsignProvider.");
    }
}
