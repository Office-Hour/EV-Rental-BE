using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
using FluentValidation;

namespace Application.Validators.BookingManagement.Queries;

public class GetBookingDetailsQueryValidator : AbstractValidator<GetBookingDetailsQuery>
{
    public GetBookingDetailsQueryValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("BookingId is required.");
    }
}
