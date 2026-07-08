using System.Security.Claims;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace AspNetNextApp.Api.Tests.Controllers.Support
{
    internal sealed class FakeTokenService : ITokenService
    {
        public ClaimsPrincipal CreateAccessToken(User user) => new(new ClaimsIdentity());

        public AuthenticationProperties CreateRefreshToken() => new();

        public bool TryValidateToken(ClaimsPrincipal principal, out Guid userId)
        {
            userId = Guid.Empty;
            return false;
        }
    }
}
