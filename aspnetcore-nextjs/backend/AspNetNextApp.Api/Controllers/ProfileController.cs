using System.Security.Claims;

using AspNetNextApp.Api.Contracts.Profile;
using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Controllers.Shared;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("")]
    [Authorize]
    public sealed class ProfileController(IAccountAuthenticationService accountAuthenticationService) : ControllerBase
    {
        [HttpGet("me")]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<AccountUserResponse> GetMe()
        {
            string? idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? email = User.FindFirstValue(ClaimTypes.Email);
            string? name = User.FindFirstValue(ClaimTypes.Name);
            string? roleValue = User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(idValue, out Guid id) || email is null || name is null || roleValue is null)
            {
                return Unauthorized(new { message = "Invalid authentication cookie." });
            }

            return Enum.TryParse(roleValue, out UserRole role)
                ? Ok(new AccountUserResponse(id, email, name, role))
                : Unauthorized(new { message = "Invalid authentication cookie." });
        }

        [HttpPut("me")]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountUserResponse>> UpdateProfileAsync(
            [FromBody] UpdateProfileRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out Guid id))
            {
                return Unauthorized(new { message = "Invalid authentication cookie." });
            }

            AccountResult<AccountUserResponse> result = await accountAuthenticationService.UpdateProfileAsync(id, request.Name, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result);
        }

        [HttpPut("me/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePasswordAsync(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out Guid id))
            {
                return Unauthorized(new { message = "Invalid authentication cookie." });
            }

            AccountResult<bool> result = await accountAuthenticationService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result, NoContent());
        }

        private bool TryGetCurrentUserId(out Guid id)
        {
            return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);
        }
    }
}
