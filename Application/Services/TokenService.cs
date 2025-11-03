using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.CustomExceptions;
using Application.Interfaces;
using Domain.Entities.BookingManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    public class TokenService(UserManager<IdentityUser> userManager, IUnitOfWork uow, IConfiguration configuration) : ITokenService
    {
        private string RefreshTokenClaimType => configuration["Jwt:REFRESH_TOKEN_CLAIM_TYPE"] ?? throw new InvalidOperationException("REFRESH_TOKEN_CLAIM_TYPE not found");
        private string RefreshTokenExpirationClaimType => configuration["Jwt:REFRESH_TOKEN_EXPIRATION_CLAIM_TYPE"] ?? throw new InvalidOperationException("REFRESH_TOKEN_EXPIRATION_CLAIM_TYPE not found");
        public string GenerateJwtToken(IEnumerable<Claim> claims, DateTime expiredAt)
        {
            var keyFromConfig = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not found"));
            if (keyFromConfig.Length <= 256) Console.WriteLine($"Jwt:Key must be greater than 256, recent size: {keyFromConfig.Length}");
            var key = new SymmetricSecurityKey(keyFromConfig);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiredAt,
                signingCredentials: creds,
                notBefore: DateTime.UtcNow
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateAccessToken(IdentityUser user, DateTime expiredAt, CancellationToken cancellationToken = default)
        {
            // Generate JWT token
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new (ClaimTypes.Name, user.UserName ?? string.Empty),
                new (ClaimTypes.Email, user.Email ?? string.Empty),
            };
            // Get user roles
            var roles = await userManager.GetRolesAsync(user);
            // Add user roles to JWT token
            foreach (var role in roles)
            {
                claims.Add(new Claim(role, "True"));
                if(role == "Renter")
                {
                    var renter = await uow.Repository<Renter>().AsQueryable().FirstOrDefaultAsync(r => r.UserId == Guid.Parse(user.Id));
                    claims.Add(new Claim("RenterId", renter!.RenterId.ToString()));
                }
                if (role == "Staff")
                {
                    var staff = await uow.Repository<Staff>().AsQueryable().FirstOrDefaultAsync(r => r.UserId == Guid.Parse(user.Id));
                    claims.Add(new Claim("StaffId", staff!.StaffId.ToString()));
                }
            }

            var accessToken = GenerateJwtToken(claims, expiredAt);
            return accessToken;
        }

        public string GenerateRefreshToken(IdentityUser user, DateTime expiredAt)
        {
            // Generate JWT token
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id)
            };

            var refreshToken = GenerateJwtToken(claims, expiredAt);

            return refreshToken;
        }

        public DateTime GenerateAccessTokenExpiration(DateTime now)
        {
            // Get expiration times of access token from configuration
            _ = double.TryParse(
                configuration["Jwt:AccessTokenExpiration"]
                ?? throw new InvalidOperationException("Jwt:ExpireIn not found in configuration")
                , out var accessTokenExpireIn);
            var accessTokenExpiration = now.AddMinutes(accessTokenExpireIn);
            return accessTokenExpiration;
        }

        public DateTime GenerateRefreshTokenExpiration(DateTime now)
        {
            // Get expiration times of access token from configuration
            _ = double.TryParse(
                configuration["Jwt:RefreshTokenExpiration"]
                ?? throw new InvalidOperationException("Jwt:ExpireIn not found in configuration")
                , out var refreshTokenExpireIn);

            var refreshTokenExpiration = now.AddDays(refreshTokenExpireIn);

            return refreshTokenExpiration;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // We want to validate the token even if it's expired
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }

        public async Task SaveRefreshToken(IdentityUser user, string refreshToken, DateTime expiredAt, CancellationToken cancellationToken = default)
        {
            // Find existing refresh token in user claims and remove it
            // One account can only have one refresh token at a time
            var refreshTokenClaim = await userManager.GetClaimsAsync(user);
            var refreshTokenClaimToRemove = refreshTokenClaim.FirstOrDefault(c => c.Type == RefreshTokenClaimType);
            var refreshTokenExpirationClaimToRemove = refreshTokenClaim.FirstOrDefault(c => c.Type == RefreshTokenExpirationClaimType);
            if (refreshTokenClaimToRemove != null)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenClaimToRemove);
            }
            if (refreshTokenExpirationClaimToRemove != null)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenExpirationClaimToRemove);
            }

            // Add refresh token to user claim in database
            // This is a simple implementation, in a real-world application you should store the refresh token in a more secure way
            await userManager.AddClaimAsync(
                user,
                new Claim(RefreshTokenClaimType, refreshToken));
            await userManager.AddClaimAsync(
                user,
                new Claim(RefreshTokenExpirationClaimType, expiredAt.ToString()));
        }

        public async Task<DateTime> GetRefreshTokenExpirationFromStore(IdentityUser user)
        {
            // Get refresh token expiration from user claims
            var claims = await userManager.GetClaimsAsync(user);
            var refreshTokenExpirationClaim = claims.FirstOrDefault(c => c.Type == RefreshTokenExpirationClaimType) ?? throw new InvalidTokenException();
            var refreshTokenExpiration = DateTime.Parse(refreshTokenExpirationClaim.Value);
            if (refreshTokenExpiration < DateTime.UtcNow)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenExpirationClaim);
                throw new Exception("Refresh Token expired");
            }
            return refreshTokenExpiration;
        }

        public async Task UpdateRefreshToken(IdentityUser user, string refreshToken, DateTime refreshTokenExpiration, CancellationToken cancellationToken)
        {
            // Find existing refresh token in user claims and remove it
            // One account can only have one refresh token at a time
            var refreshTokenClaim = await userManager.GetClaimsAsync(user);
            var refreshTokenClaimToRemove = refreshTokenClaim.FirstOrDefault(c => c.Type == RefreshTokenClaimType);
            var refreshTokenExpirationClaimToRemove = refreshTokenClaim.FirstOrDefault(c => c.Type == RefreshTokenExpirationClaimType);
            
            if (refreshTokenClaimToRemove != null)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenClaimToRemove);
            }
            
            if (refreshTokenExpirationClaimToRemove != null)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenExpirationClaimToRemove);
            }
            // Add new refresh token to user claim in database
            await userManager.AddClaimAsync(
                user,
                new Claim(RefreshTokenClaimType, refreshToken));
            await userManager.AddClaimAsync(
                user,
                new Claim(RefreshTokenExpirationClaimType, refreshTokenExpiration.ToString()));
        }

        public async Task RemoveRefreshToken(IdentityUser user, CancellationToken cancellationToken)
        {
            // Find existing refresh token in user claims and remove it
            // One account can only have one refresh token at a time
            var refreshTokenClaim = await userManager.GetClaimsAsync(user);
            var refreshTokenClaimToRemove = refreshTokenClaim.FirstOrDefault(c => c.Type == RefreshTokenClaimType);
            var refreshTokenExpirationClaimToRemove = refreshTokenClaim.FirstOrDefault(c => c.Type == RefreshTokenExpirationClaimType);
            if (refreshTokenClaimToRemove != null)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenClaimToRemove);
            }
            if (refreshTokenExpirationClaimToRemove != null)
            {
                await userManager.RemoveClaimAsync(user, refreshTokenExpirationClaimToRemove);
            }
        }
    }
}
