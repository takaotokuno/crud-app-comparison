using System.Security.Claims;
using AspNetNextApp.Api.Entities;
using Microsoft.AspNetCore.Authentication;

namespace AspNetNextApp.Api.Services.Tokens
{
    public interface ITokenService
    {
        ClaimsPrincipal CreateAccessToken(User user);

        AuthenticationProperties CreateRefreshToken();

        bool TryValidateToken(ClaimsPrincipal principal, out Guid userId);
    }
}
