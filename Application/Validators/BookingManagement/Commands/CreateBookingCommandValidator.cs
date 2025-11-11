using Application.UseCases.BookingManagement.Commands.CreateBooking;
using Domain.Enums;
using FluentValidation;

namespace Application.Validators.BookingManagement.Commands;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.RenterId)
            .NotEmpty().WithMessage("RenterId is required.");

        RuleFor(x => x.DepositFeeDto.Type)
            .Equal(FeeType.Deposit).WithMessage("Deposit fee type is required.");
        RuleFor(x => x.DepositFeeDto.Amount)
            .GreaterThan(0).WithMessage("Deposit amount must be greater than 0.");

        RuleFor(x => x.CreateBookingDto.VehicleAtStationId)
            .NotEmpty().WithMessage("VehicleAtStationId is required.");
        RuleFor(x => x.CreateBookingDto.StartTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("StartTime must be in the future.");
        RuleFor(x => x.CreateBookingDto.EndTime)
            .GreaterThan(x => x.CreateBookingDto.StartTime).WithMessage("EndTime must be after StartTime.");
    }
}
