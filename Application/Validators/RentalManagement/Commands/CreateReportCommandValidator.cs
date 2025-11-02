using Application.UseCases.RentalManagement.Commands.CreateReport;
using FluentValidation;

namespace Application.Validators.RentalManagement.Commands;

public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("InspectionId is required.");
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}
