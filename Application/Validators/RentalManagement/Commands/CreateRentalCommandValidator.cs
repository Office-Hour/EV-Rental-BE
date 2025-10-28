using Application.UseCases.RentalManagement.Commands.CreateRental;
using FluentValidation;

namespace Application.Validators.RentalManagement.Commands;

public class CreateRentalCommandValidator : AbstractValidator<CreateRentalCommand>
{
    public CreateRentalCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("BookingId is required.");
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("VehicleId is required.");
        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime).WithMessage("StartTime must be earlier than EndTime.");
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be later than StartTime.");
    }
}
