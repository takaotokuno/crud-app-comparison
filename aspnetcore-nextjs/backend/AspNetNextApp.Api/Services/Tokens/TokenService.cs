using System.Security.Claims;
using AspNetNextApp.Api.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AspNetNextApp.Api.Services.Tokens
{
    public sealed class TokenService : ITokenService
    {
        public ClaimsPrincipal CreateAccessToken(User user)
        {
            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString()),
            ];

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        public AuthenticationProperties CreateRefreshToken()
        {
            return new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
            };
        }

        public bool TryValidateToken(ClaimsPrincipal principal, out Guid userId)
        {
            return Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }
    }
}
