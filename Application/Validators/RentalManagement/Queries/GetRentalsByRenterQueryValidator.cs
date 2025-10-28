using Application.UseCases.RentalManagement.Queries.GetRentalByRenter;
using FluentValidation;

namespace Application.Validators.RentalManagement.Queries;

public class GetRentalsByRenterQueryValidator : AbstractValidator<GetRentalsByRenterQuery>
{
    public GetRentalsByRenterQueryValidator()
    {
        RuleFor(x => x.RenterId)
            .NotEmpty().WithMessage("RenterId is required.");
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.");
    }
}
