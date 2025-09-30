namespace WebApp.UIAuthService;

// IUiAuthService.cs
public interface IUiAuthService
{
    Task LoginAsync(string email, string password, bool rememberMe, CancellationToken ct);
    Task RegisterAsync(string email, string? userName, string password, string confirmPassword, CancellationToken ct, bool signInAfter = true);
    Task LogoutAsync(CancellationToken ct);
}

