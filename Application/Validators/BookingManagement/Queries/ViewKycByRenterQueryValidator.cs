using Application.UseCases.BookingManagement.Queries.ViewKycByRenter;
using FluentValidation;

namespace Application.Validators.BookingManagement.Queries;

public class ViewKycByRenterQueryValidator: AbstractValidator<ViewKycByRenterQuery>
{
    public ViewKycByRenterQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100.");
        RuleFor(x => x.RenterId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
