using Application.UseCases.RentalManagement.Queries.GetRentalByBookingId;
using FluentValidation;

namespace Application.Validators.RentalManagement.Queries;

public class GetRentalByBookingIdQueryValidator : AbstractValidator<GetRentalByBookingIdQuery>
{
    public GetRentalByBookingIdQueryValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("BookingId is required.");
    }
}
