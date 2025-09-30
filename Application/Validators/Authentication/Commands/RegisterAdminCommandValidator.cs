using Application.UseCases.Authentication.Commands.RegisterAdmin;
using FluentValidation;

namespace Application.Validators.Authentication.Commands;

public class RegisterAdminCommandValidator : AbstractValidator<RegisterAdminCommand>
{
    public RegisterAdminCommandValidator()
    {
        RuleFor(x => x.IdentityUserRole)
            .NotEmpty().WithMessage("Role is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}
