using Application.UseCases.BookingManagement.Commands.CheckinBooking;
using Domain.Enums;
using FluentValidation;

namespace Application.Validators.BookingManagement.Commands;

public class CheckinBookingCommandValidator : AbstractValidator<CheckinBookingCommand>
{
    public CheckinBookingCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("BookingId is required.");
        RuleFor(x => x.VerifiedByStaffId)
            .NotEmpty().WithMessage("VerifiedByStaffId is required.");
        RuleFor(x => x.BookingVerificationStatus)
            .IsInEnum().WithMessage("Invalid BookingVerificationStatus.");
        When(x => x.BookingVerificationStatus == BookingVerificationStatus.Rejected_Mismatch || 
                  x.BookingVerificationStatus == BookingVerificationStatus.Rejected_Other, 
            () =>
        {
            RuleFor(x => x.CancelReason)
                .NotEmpty().WithMessage("CancelReason is required when the booking is rejected.");
        });
    }
}
