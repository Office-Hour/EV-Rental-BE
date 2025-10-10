using Application.UseCases.StationManagement.Queries.ViewStation;
using FluentValidation;

namespace Application.Validators.StationManagement.Queries;

public class ViewStationQueryValidator : AbstractValidator<ViewStationQuery>
{
    public ViewStationQueryValidator()
    {
        RuleFor(x => x.Paging.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100.");

        RuleFor(x => x.Paging.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
    }
}
