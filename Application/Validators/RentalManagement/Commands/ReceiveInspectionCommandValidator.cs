using Application.UseCases.RentalManagement.Commands.ReceiveInspection;
using FluentValidation;

namespace Application.Validators.RentalManagement.Commands;

public class ReceiveInspectionCommandValidator : AbstractValidator<ReceiveInspectionCommand>
{
    public ReceiveInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("InspectionId is required.");
        RuleFor(x => x.RentalId)
            .NotEmpty().WithMessage("RentalId is required.");
        RuleFor(x => x.CurrentBatteryCapacityKwh)
            .GreaterThanOrEqualTo(0).WithMessage("CurrentBatteryCapacityKwh must be non-negative.");
        RuleFor(x => x.InspectedAt)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("InspectedAt cannot be in the future.");
        RuleFor(x => x.InspectorStaffId)
            .NotEmpty().WithMessage("InspectorStaffId is required.");
    }
}
