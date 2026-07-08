using System.Security.Claims;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace AspNetNextApp.Api.Tests.Controllers.Support
{
    internal sealed class FakeTokenService : ITokenService
    {
        public ClaimsPrincipal CreateAccessToken(User user)
        {
            return new(new ClaimsIdentity());
        }

        public AuthenticationProperties CreateRefreshToken()
        {
            return new();
        }

        public bool TryValidateToken(ClaimsPrincipal principal, out Guid userId)
        {
            userId = Guid.Empty;
            return false;
        }
    }
}
