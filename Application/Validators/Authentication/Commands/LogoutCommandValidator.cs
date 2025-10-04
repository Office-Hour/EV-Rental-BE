using Application.UseCases.Authentication.Commands.Logout;
using FluentValidation;

namespace Application.Validators.Authentication.Commands
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
