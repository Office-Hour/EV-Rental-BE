using Application.UseCases.RentalManagement.Queries.GetRentalDetails;
using FluentValidation;

namespace Application.Validators.RentalManagement.Queries;

public class GetRentalDetailsQueryValidator : AbstractValidator<GetRentalDetailsQuery>
{
    public GetRentalDetailsQueryValidator()
    {
        RuleFor(x => x.RentalId)
            .NotEmpty().WithMessage("RentalId is required.");
    }
}
