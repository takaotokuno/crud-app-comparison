using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class UsersControllerTests
    {
        [Fact]
        public async Task CreateAsync_WhenServiceSucceedsReturnsCreatedUser()
        {
            CapturingUserService service = new();
            UsersController controller = new(service);
            CreateUserRequest request = new("staff@example.com", "password123", "Staff User", UserRole.Staff);

            ActionResult<AccountUserResponse> actionResult = await controller.CreateAsync(request, CancellationToken.None);

            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(UsersController.GetAsync), createdResult.ActionName);
            AccountUserResponse response = Assert.IsType<AccountUserResponse>(createdResult.Value);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(request.Name, response.Name);
            Assert.Equal(request.Role, response.Role);
            Assert.Equal(request.Password, service.CapturedPassword);
        }

        [Fact]
        public void UsersController_RequiresAuthenticatedAdmins()
        {
            Assert.Contains(
                typeof(UsersController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true),
                attribute => attribute is AuthorizeAttribute);

            UserRoleAttribute roleAttribute = Assert.Single(
                typeof(UsersController).GetCustomAttributes(typeof(UserRoleAttribute), inherit: true).Cast<UserRoleAttribute>());
            Assert.Equal(UserRole.Admin.ToString(), roleAttribute.Roles);
        }

        [Fact]
        public async Task ListAsync_ForwardsSearchFilterSortAndPaging()
        {
            CapturingUserService service = new();
            UsersController controller = new(service);
            ListUsersRequest request = new()
            {
                Query = "alice",
                Role = UserRole.Staff,
                SortBy = "name",
                SortDirection = "desc",
                Page = 2,
                PageSize = 10,
            };

            _ = await controller.ListAsync(request, CancellationToken.None);

            Assert.Equal(new ListUsersQuery("alice", UserRole.Staff, "name", "desc", 2, 10), service.CapturedListQuery);
        }

        [Theory]
        [InlineData(AccountErrorType.Validation)]
        [InlineData(null)]
        public async Task ListAsync_WhenServiceReturnsInvalidFailureReturnsBadRequest(AccountErrorType? errorType)
        {
            CapturingUserService service = new()
            {
                ListResult = new AccountResult<AccountUserListResponse>(null, false, "Invalid user query.", errorType)
            };
            UsersController controller = new(service);

            ActionResult<AccountUserListResponse> actionResult = await controller.ListAsync(new ListUsersRequest(), CancellationToken.None);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        [Theory]
        [InlineData(nameof(UsersController.ListAsync), null)]
        [InlineData(nameof(UsersController.CreateAsync), null)]
        [InlineData(nameof(UsersController.GetAsync), "{id:guid}")]
        [InlineData(nameof(UsersController.UpdateAsync), "{id:guid}")]
        [InlineData(nameof(UsersController.DeleteAsync), "{id:guid}")]
        [InlineData(nameof(UsersController.ChangeRoleAsync), "{id:guid}/role")]
        public void UserEndpoints_UseStandardRouteTemplates(string actionName, string? routeTemplate)
        {
            System.Reflection.MethodInfo method = typeof(UsersController).GetMethods()
                .Single(method => method.Name == actionName);

            HttpMethodAttribute attribute = Assert.Single(
                method.GetCustomAttributes(typeof(HttpMethodAttribute), inherit: true).Cast<HttpMethodAttribute>());

            Assert.Equal(routeTemplate, attribute.Template);
        }

        private sealed class CapturingUserService : IUserService
        {
            public string? CapturedPassword { get; private set; }
            public ListUsersQuery? CapturedListQuery { get; private set; }
            public AccountResult<AccountUserListResponse> ListResult { get; init; } =
                AccountResult<AccountUserListResponse>.Success(new AccountUserListResponse([], 1, 20, 0));

            public Task<AccountResult<User>> CreateUserAsync(string email, string password, string name, UserRole role, CancellationToken cancellationToken = default)
            {
                CapturedPassword = password;
                User user = new() { Email = email, Name = name, Role = role };
                return Task.FromResult(AccountResult<User>.Success(user));
            }

            public Task<AccountResult<AccountUserListResponse>> ListUsersAsync(ListUsersQuery query, CancellationToken cancellationToken = default)
            {
                CapturedListQuery = query;
                return Task.FromResult(ListResult);
            }
            public Task<AccountResult<AccountUserResponse>> GetUserAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<AccountResult<AccountUserResponse>> UpdateUserAsync(Guid id, string email, string name, UserRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<AccountResult<bool>> DeleteUserAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<AccountResult<AccountUserResponse>> ChangeUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<bool> IsEmailInUseAsync(string email, Guid? excludedUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}
