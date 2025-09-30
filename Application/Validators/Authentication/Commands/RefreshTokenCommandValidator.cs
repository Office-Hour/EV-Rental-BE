using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.UseCases.Authentication.Commands.RefreshToken;
using FluentValidation;

namespace Application.Validators.Authentication.Commands
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Refresh token is required.")
                .Must(token => new JwtSecurityTokenHandler().CanReadToken(token))
                .WithMessage("Refresh token is invalid.");
        }
    }
}
