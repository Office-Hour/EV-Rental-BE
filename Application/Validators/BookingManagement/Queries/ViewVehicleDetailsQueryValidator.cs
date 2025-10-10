using Application.UseCases.BookingManagement.Queries.ViewVehicleDetails;
using FluentValidation;

namespace Application.Validators.BookingManagement.Queries;

public class ViewVehicleDetailsQueryValidator : AbstractValidator<ViewVehicleDetailsQuery>
{
    public ViewVehicleDetailsQueryValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("VehicleId is required.");
    }
}
