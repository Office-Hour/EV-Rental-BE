using Application.UseCases.RentalManagement.Commands.ReceiveVehicle;
using FluentValidation;

namespace Application.Validators.RentalManagement.Commands;

public class ReceiveVehicleCommandValidator : AbstractValidator<ReceiveVehicleCommand>
{
    public ReceiveVehicleCommandValidator()
    {
        RuleFor(x => x.RentalId)
            .NotEmpty().WithMessage("RentalId is required.");

    }
}
