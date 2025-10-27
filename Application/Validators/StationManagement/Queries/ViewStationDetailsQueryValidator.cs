using Application.UseCases.StationManagement.Queries.ViewStationDetails;
using FluentValidation;

namespace Application.Validators.StationManagement.Queries;

public class ViewStationDetailsQueryValidator : AbstractValidator<ViewStationDetailsQuery>
{
    public ViewStationDetailsQueryValidator()
    {
        RuleFor(x => x.StationId)
            .NotEmpty().WithMessage("StationId is required.");
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.");
    }
}
