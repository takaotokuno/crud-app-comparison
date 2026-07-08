using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Controllers.Shared;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize]
    [UserRole(UserRole.Admin)]
    public sealed class UsersController(IUserService userService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(AccountUserListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<AccountUserListResponse>> ListAsync(
            [FromQuery] int page = 1,
            [FromQuery(Name = "page_size")] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            AccountResult<AccountUserListResponse> result = await userService.ListUsersAsync(page, pageSize, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountUserResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            AccountResult<AccountUserResponse> result = await userService.GetUserAsync(id, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AccountUserResponse>> UpdateAsync(
            Guid id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            AccountResult<AccountUserResponse> result = await userService.UpdateUserAsync(
                id,
                request.Email,
                request.Name,
                request.Role,
                cancellationToken);

            return AccountControllerResults.ToActionResult(this, result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            AccountResult<bool> result = await userService.DeleteUserAsync(id, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result, NoContent());
        }

        [HttpPut("{id:guid}/role")]
        [ProducesResponseType(typeof(AccountUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AccountUserResponse>> ChangeRoleAsync(
            Guid id,
            [FromBody] ChangeUserRoleRequest request,
            CancellationToken cancellationToken)
        {
            AccountResult<AccountUserResponse> result = await userService.ChangeUserRoleAsync(id, request.Role, cancellationToken);
            return AccountControllerResults.ToActionResult(this, result);
        }
    }
}
