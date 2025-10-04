using Application.UseCases.Profile.Queries.GetProfile;
using FluentValidation;

namespace Application.Validators.Profile.Queries
{
    public class GetProfileQueryValidator : AbstractValidator<GetProfileQuery>
    {
        public GetProfileQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}
