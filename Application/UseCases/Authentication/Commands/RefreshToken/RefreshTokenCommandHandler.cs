using System.Security.Claims;
using Application.CustomExceptions;
using Application.DTOs.Authentication;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService)
        : IRequestHandler<RefreshTokenCommand, AuthDto>
    {
        public async Task<AuthDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var claimsPrinciple = tokenService.GetPrincipalFromExpiredToken(request.RefreshToken) ?? throw new InvalidTokenException();
            var userId = claimsPrinciple.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidTokenException();
            var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidTokenException();

            // Get old refresh token expiration from database and check if it is expired
            var refreshTokenExpiration = await tokenService.GetRefreshTokenExpirationFromStore(user);
            
            var now = DateTime.UtcNow;
            var accessTokenExpiration = tokenService.GenerateAccessTokenExpiration(now);

            var accessToken = await tokenService.GenerateAccessToken(user, accessTokenExpiration, cancellationToken);
            var refreshToken = tokenService.GenerateRefreshToken(user, refreshTokenExpiration);

            // Update refresh token to database, remove old refresh token
            await tokenService.UpdateRefreshToken(user, refreshToken, refreshTokenExpiration, cancellationToken);
            return new AuthDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };
        }
    }
}
