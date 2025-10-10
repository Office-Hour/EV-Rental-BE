using Application.UseCases.BookingManagement.Queries.GetBookingByRenter;
using FluentValidation;

namespace Application.Validators.BookingManagement.Queries;

public class GetBookingByRenterQueryValidator : AbstractValidator<GetBookingByRenterQuery>
{
    public GetBookingByRenterQueryValidator()
    {
        RuleFor(x => x.RenterId)
            .NotEmpty().WithMessage("RenterId is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage($"PageSize must be between 1 and 100.");
    }
}
