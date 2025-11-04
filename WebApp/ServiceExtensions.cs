using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistence;

namespace WebApp;

public static class ServiceExtensions
{
    // Full Identity (cookies) for Razor Pages
    public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;

                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configure the Identity application cookie that AddIdentity already registered
        services.ConfigureApplicationCookie(opt =>
        {
            // If your pages are under /Account/*, use "/Account/Login".
            // If you scaffolded the Identity Area, keep "/Identity/Account/Login".
            opt.LoginPath = "/Identity/Account/Login";          // <-- change if you use the Identity Area
            opt.LogoutPath = "/Identity/Account/Logout";
            opt.AccessDeniedPath = "/Identity/Account/AccessDenied";

            opt.SlidingExpiration = true;
            opt.ExpireTimeSpan = TimeSpan.FromHours(8);
            opt.Cookie.HttpOnly = true;
            opt.Cookie.Name = "WebApp.Identity";
        });

        return services;
    }

    // Do NOT re-add the Identity.Application cookie scheme here.
    // Only add optional extra schemes (e.g., JWT) if you need them.
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Optional: add a separate JWT scheme for API endpoints
        var key = configuration["Jwt:Key"];
        if (!string.IsNullOrWhiteSpace(key))
        {
            services.AddAuthentication() // uses existing defaults from AddIdentity
                .AddJwtBearer("ApiJwt", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateLifetime = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("User", p => p.RequireClaim(ClaimTypes.NameIdentifier))
            .AddPolicy("Admin", p => p.RequireClaim("Admin"));

        // If you added the "ApiJwt" scheme above and want a policy that uses it:
        services.AddAuthorizationBuilder()
            .AddPolicy("Api", p =>
            {
                p.AddAuthenticationSchemes("ApiJwt");
                p.RequireAuthenticatedUser();
            });

        return services;
    }
}
