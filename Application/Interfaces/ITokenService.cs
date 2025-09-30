using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        public string GenerateJwtToken(IEnumerable<Claim> claims, DateTime expiredAt);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        public Task<string> GenerateAccessToken(IdentityUser user, DateTime expiredAt, CancellationToken cancellationToken = default);
        public string GenerateRefreshToken(IdentityUser user, DateTime expiredAt);
        public Task SaveRefreshToken(IdentityUser user, string refreshToken, DateTime expiredAt, CancellationToken cancellationToken = default);
        public Task UpdateRefreshToken(IdentityUser user, string refreshToken, DateTime refreshTokenExpiration, CancellationToken cancellationToken = default);
        public Task RemoveRefreshToken(IdentityUser user, CancellationToken cancellationToken);
        public Task<DateTime> GetRefreshTokenExpirationFromStore(IdentityUser user);
        public DateTime GenerateAccessTokenExpiration(DateTime now);
        public DateTime GenerateRefreshTokenExpiration(DateTime now);
    }
}