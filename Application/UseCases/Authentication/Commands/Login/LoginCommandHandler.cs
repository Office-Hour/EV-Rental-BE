using Application.CustomExceptions;
using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Authentication.Commands.Login;

public class LoginCommandHandler(
    UserManager<IdentityUser> userManager,
    ITokenService tokenService,
    ILogger<LoginCommandHandler> logger)
: IRequestHandler<LoginCommand, AuthDto>
{
    public async Task<AuthDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Get user by email, throw exception if not found
        var user = await userManager.FindByEmailAsync(request.Email) ?? throw new LoginFailException();
        
        // Check if user is locked out
        if (user.LockoutEnabled && user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        {
            // Log the lockout event
            logger.LogWarning("User {UserId} is locked out until {LockoutEnd}", user.Id, user.LockoutEnd);
            throw new LockoutException();
        }

        // Validate password, increase failed access attempts if invalid and throw exception
        var result = await userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            await userManager.AccessFailedAsync(user);
            throw new LoginFailException();
        }

        // Reset access failed attempts
        await userManager.ResetAccessFailedCountAsync(user);

        // Generate access and refresh tokens
        var now = DateTime.UtcNow;

        // Get expiration times of access token and refresh token from configuration
        var accessTokenExpiration = tokenService.GenerateAccessTokenExpiration(now);
        var refreshTokenExpiration = tokenService.GenerateRefreshTokenExpiration(now);

        // Generate JWT tokens
        var refreshToken = tokenService.GenerateRefreshToken(user, refreshTokenExpiration);
        var accessToken = await tokenService.GenerateAccessToken(user, accessTokenExpiration, cancellationToken);

        // Save refresh token to database
        await tokenService.SaveRefreshToken(user, refreshToken, refreshTokenExpiration, cancellationToken);

        // Create AuthDto object to return
        return new AuthDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = accessTokenExpiration,
            RefreshTokenExpiration = refreshTokenExpiration
        };
    }
}
