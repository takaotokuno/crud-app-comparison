using System.Security.Claims;

using AspNetNextApp.Api.Contracts.Auth;
using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Controllers.Shared;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("")]
    [Authorize]
    public sealed class AuthController(
        IAccountAuthenticationService accountAuthenticationService) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AccountUserResponse>> RegisterAsync(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            AccountRegistrationResult result = await accountAuthenticationService.RegisterAsync(
                request.Email,
                request.Password,
                request.Name,
                cancellationToken);

            return result.Succeeded && result.User is not null
                ? CreatedAtAction(nameof(ProfileController.GetMe), "Profile", null, ToResponse(result.User))
                : Conflict(new { message = result.ErrorMessage });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccountUserResponse>> LoginAsync(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            User? user = await accountAuthenticationService.AuthenticateAsync(
                request.Email,
                request.Password,
                cancellationToken);

            if (user is null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString()),
            ];

            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                });

            return Ok(ToResponse(user));
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        [HttpPost("password-reset/request")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RequestPasswordResetAsync(
            [FromBody] RequestPasswordResetRequest request,
            CancellationToken cancellationToken)
        {
            await accountAuthenticationService.RequestPasswordResetAsync(request.Email, cancellationToken);
            return NoContent();
        }

        [HttpPost("password-reset")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPasswordAsync(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            AccountResult<bool> result = await accountAuthenticationService.ResetPasswordAsync(request.Email, request.NewPassword, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result, NoContent());
        }

        private static AccountUserResponse ToResponse(User user)
        {
            return new(user.Id, user.Email, user.Name, user.Role);
        }
    }
}
