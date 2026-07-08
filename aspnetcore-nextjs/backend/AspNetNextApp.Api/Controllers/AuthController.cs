using AspNetNextApp.Api.Contracts.Auth;
using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Controllers.Shared;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Auth;
using AspNetNextApp.Api.Services.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("")]
    [Authorize]
    public sealed class AuthController(
        IAuthService authService,
        ITokenService tokenService) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AccountUserResponse>> RegisterAsync(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            AccountRegistrationResult result = await authService.RegisterAsync(
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
            User? user = await authService.LoginAsync(
                request.Email,
                request.Password,
                cancellationToken);

            if (user is null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            await HttpContext.SignInAsync(
                tokenService.CreateAccessToken(user),
                tokenService.CreateRefreshToken());

            return Ok(ToResponse(user));
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync()
        {
            await authService.LogoutAsync();
            await HttpContext.SignOutAsync();
            return NoContent();
        }

        [HttpPost("password-reset/request")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RequestPasswordResetAsync(
            [FromBody] RequestPasswordResetRequest request,
            CancellationToken cancellationToken)
        {
            await authService.RequestPasswordResetAsync(request.Email, cancellationToken);
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
            AccountResult<bool> result = await authService.ResetPasswordAsync(request.Email, request.NewPassword, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result, NoContent());
        }

        private static AccountUserResponse ToResponse(User user)
        {
            return new(user.Id, user.Email, user.Name, user.Role);
        }
    }
}
