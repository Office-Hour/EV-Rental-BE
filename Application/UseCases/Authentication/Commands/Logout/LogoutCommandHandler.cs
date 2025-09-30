using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.CustomExceptions;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Authentication.Commands.Logout
{
    public class LogoutCommandHandler(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService
        ) : IRequestHandler<LogoutCommand>
    {
        public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var claimsPrinciple = tokenService.GetPrincipalFromExpiredToken(request.RefreshToken) ?? throw new InvalidTokenException();
            var userId = claimsPrinciple.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidTokenException();
            var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidTokenException();

            // Remove refresh token from database
            await tokenService.RemoveRefreshToken(user, cancellationToken);
        }
    }
}
