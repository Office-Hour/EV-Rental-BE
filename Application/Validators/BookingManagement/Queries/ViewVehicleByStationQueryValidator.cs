using Application.UseCases.BookingManagement.Queries.ViewVehiclesByStation;
using FluentValidation;

namespace Application.Validators.BookingManagement.Queries;

public class ViewVehicleByStationQueryValidator : AbstractValidator<ViewVehiclesByStationQuery>
{
    public ViewVehicleByStationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must be less than or equal to 100.");

        RuleFor(x => x.StationId)
            .NotEmpty().WithMessage("StationId is required.");
    }
}
