using Application.UseCases.BookingManagement.Commands.CancelChecking;
using FluentValidation;

namespace Application.Validators.BookingManagement.Commands;

public class CancelCheckinCommandValidator : AbstractValidator<CancelCheckinCommand>
{
    public CancelCheckinCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty().WithMessage("BookingId is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.CancelReason).NotEmpty().MaximumLength(500).WithMessage("CancelReason cannot exceed 500 characters.");
        RuleFor(x => x.CancelCheckinCode).NotEmpty().WithMessage("CancelCheckinCode is required.");
    }
}
