using Application.UseCases.StationManagement.Queries.FilterStation;
using FluentValidation;

namespace Application.Validators.StationManagement.Queries;

public class FilterStationQueryValidator : AbstractValidator<FilterStationQuery>
{
    public FilterStationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100.");
    }
}
