using Application.UseCases.Profile.Queries.GetRenterProfile;
using FluentValidation;

namespace Application.Validators.Profile.Queries;

public class GetRenterProfileQueryValidator : AbstractValidator<GetRenterProfileQuery>
{
    public GetRenterProfileQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("RenterId is required.");
    }
}
