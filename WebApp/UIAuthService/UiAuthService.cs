// UiAuthService.cs
using Application.UseCases.Authentication.Commands.Login;
using Application.UseCases.Authentication.Commands.Logout;
using Application.UseCases.Authentication.Commands.Register;
using Application.Validators.Authentication.Commands;
using Azure.Core;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebApp.UIAuthService;

public sealed class UiAuthService : IUiAuthService
{
    private readonly IMediator _mediator;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<UiAuthService> _logger;

    public UiAuthService(IMediator mediator,
                         UserManager<IdentityUser> userManager,
                         IHttpContextAccessor http,
                         ILogger<UiAuthService> logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _http = http;
        _logger = logger;
    }

    public async Task RegisterAsync(string email, string? userName, string password, string confirmPassword, CancellationToken ct, bool signInAfter = true)
    {
        var command = new RegisterCommand { Email = email, UserName = userName, Password = password, ConfirmPassword = confirmPassword };
        var validator = new RegisterCommandValidator();
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new FluentValidation.ValidationException(errors);
        }
        await _mediator.Send(command, ct);

        if (signInAfter)
            await LoginAsync(email, password, rememberMe: false, ct);
    }

    public async Task LoginAsync(string email, string password, bool rememberMe, CancellationToken ct)
    {
        // 1) Use your existing MediatR handler to validate & issue tokens
        var command = new LoginCommand { Email = email, Password = password };
        var validator = new LoginCommandValidator();
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new FluentValidation.ValidationException(errors);
        }
        var auth = await _mediator.Send(command, ct);

        // 2) Build a ClaimsPrincipal for the cookie (from Identity store)
        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new InvalidOperationException("User not found after login.");
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? email),
            new Claim(ClaimTypes.Email, email)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);

        // 3) Sign the application cookie (used by Razor Pages)
        var props = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };
        await _http.HttpContext!.SignInAsync(IdentityConstants.ApplicationScheme, principal, props);

        // 4) Store the refresh token as a secure HttpOnly cookie
        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = auth.RefreshTokenExpiration
        };
        _http.HttpContext.Response.Cookies.Append("refreshToken", auth.RefreshToken, refreshOptions);
    }

    public async Task LogoutAsync(CancellationToken ct)
    {
        // Read refresh token cookie (if present) and call your logout handler
        if (_http.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out var rt) && !string.IsNullOrWhiteSpace(rt))
        {
            try
            {
                await _mediator.Send(new LogoutCommand { RefreshToken = rt }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke refresh token on logout");
            }
        }

        // Clear cookies
        _http.HttpContext.Response.Cookies.Delete("refreshToken");
        await _http.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}
