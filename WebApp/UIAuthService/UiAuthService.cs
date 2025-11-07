// UiAuthService.cs
using Application.Interfaces;
using Application.UseCases.Authentication.Commands.Login;
using Application.UseCases.Authentication.Commands.Logout;
using Application.UseCases.Authentication.Commands.Register;
using Application.UseCases.Profile.Queries.GetRenterProfile;
using Application.Validators.Authentication.Commands;
using Domain.Entities.BookingManagement;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
//using System.Security.Claims;

namespace WebApp.UIAuthService;

public sealed class UiAuthService(IMediator mediator,
                     UserManager<IdentityUser> userManager,
                     IHttpContextAccessor http,
                     ILogger<UiAuthService> logger,
                     IUnitOfWork uow) : IUiAuthService
{
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
        await mediator.Send(command, ct);

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
        var auth = await mediator.Send(command, ct);

        // 2) Build a ClaimsPrincipal for the cookie (from Identity store)
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new InvalidOperationException("User not found after login.");
        var roles = await userManager.GetRolesAsync(user);


        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? email),
            new Claim(ClaimTypes.Email, email)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (roles.Any(r => r == "Renter"))
        {
            var getRenterProfileCommand = new GetRenterProfileQuery
            {
                UserId = Guid.Parse(user.Id)
            };
            var renter = await mediator.Send(getRenterProfileCommand);

            var renterIdClaim = new Claim("RenterId", renter.RenterId.ToString());
            claims.Add(renterIdClaim);
        }
        if(roles.Any(r => r == "Staff"))
        {
            var staff = await uow.Repository<Staff>()
                .AsQueryable()
                .FirstOrDefaultAsync(s => s.UserId == Guid.Parse(user.Id));
            var staffIdClaim = new Claim("StaffId", staff.StaffId.ToString());
            claims.Add(staffIdClaim);
        }

        var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
        var principal = new ClaimsPrincipal(identity);

        // 3) Sign the application cookie (used by Razor Pages)
        var props = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };
        await http.HttpContext!.SignInAsync(IdentityConstants.ApplicationScheme, principal, props);

        // 4) Store the refresh token as a secure HttpOnly cookie
        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = auth.RefreshTokenExpiration
        };
        http.HttpContext!.Response.Cookies.Append("refreshToken", auth.RefreshToken, refreshOptions);
    }

    public async Task LogoutAsync(CancellationToken ct)
    {
        // Read refresh token cookie (if present) and call your logout handler
        if (http.HttpContext!.Request.Cookies.TryGetValue("refreshToken", out var rt) && !string.IsNullOrWhiteSpace(rt))
        {
            try
            {
                await mediator.Send(new LogoutCommand { RefreshToken = rt }, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to revoke refresh token on logout");
            }
        }

        // Clear cookies
        http.HttpContext.Response.Cookies.Delete("refreshToken");
        await http.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}
