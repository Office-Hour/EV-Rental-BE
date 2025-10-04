using Application.UseCases.Profile.Commands.UpdateProfile;
using FluentValidation;

namespace Application.Validators.Profile.Commands
{
    public class UpdateProfileCommandValidator: AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");
            RuleFor(x => x.UserName)
                .MinimumLength(3)
                .Unless(x => string.IsNullOrEmpty(x.UserName))
                .WithMessage("Username must be at least 3 characters long.");
            RuleFor(x => x.Email)
                .EmailAddress()
                .Unless(x => string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format.");
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .Unless(x => string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format.");
        }
    }
}
