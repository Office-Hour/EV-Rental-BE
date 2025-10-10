using Application.UseCases.BookingManagement.Commands.UploadKyc;
using FluentValidation;

namespace Application.Validators.BookingManagement.Commands;

public class UploadKycCommandValidator : AbstractValidator<UploadKycCommand>
{
    public UploadKycCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid KYC type.");
        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Document number is required.")
            .MaximumLength(50).WithMessage("Document number cannot exceed 50 characters.");
        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow).When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be in the future if provided.");
    }
}
