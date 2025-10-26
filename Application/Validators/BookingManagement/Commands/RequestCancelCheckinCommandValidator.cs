using Application.UseCases.BookingManagement.Commands.RequestCancelCheckin;
using FluentValidation;

namespace Application.Validators.BookingManagement.Commands;

public class RequestCancelCheckinCommandValidator : AbstractValidator<RequestCancelCheckinCommand>
{
    public RequestCancelCheckinCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty().WithMessage("BookingId is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}
