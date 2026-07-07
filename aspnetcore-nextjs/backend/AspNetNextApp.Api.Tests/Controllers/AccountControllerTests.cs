using AspNetNextApp.Api.Contracts.Account;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class AccountControllerTests
    {
        [Fact]
        public async Task RegisterAsync_WhenServiceSucceedsReturnsCreatedUser()
        {
            User user = new()
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                Name = "New User",
            };
            CapturingAccountAuthenticationService service = new()
            {
                RegisterResult = AccountRegistrationResult.Success(user),
            };
            AccountController controller = new(service);
            RegisterRequest request = new("user@example.com", "password123", "New User");

            ActionResult<AccountUserResponse> actionResult = await controller.RegisterAsync(request, CancellationToken.None);

            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(AccountController.GetMe), createdResult.ActionName);
            AccountUserResponse response = Assert.IsType<AccountUserResponse>(createdResult.Value);
            Assert.Equal(user.Id, response.Id);
            Assert.Equal(user.Email, response.Email);
            Assert.Equal(user.Name, response.Name);
            Assert.Equal(user.Role, response.Role);
            Assert.Equal(request.Email, service.CapturedRegisterEmail);
            Assert.Equal(request.Password, service.CapturedRegisterPassword);
            Assert.Equal(request.Name, service.CapturedRegisterName);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExistsReturnsConflictWithMessage()
        {
            const string error = "Email is already registered.";
            CapturingAccountAuthenticationService service = new()
            {
                RegisterResult = AccountRegistrationResult.Failure(error),
            };
            AccountController controller = new(service);
            RegisterRequest request = new("user@example.com", "password123", "New User");

            ActionResult<AccountUserResponse> actionResult = await controller.RegisterAsync(request, CancellationToken.None);

            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
            Assert.Equal(error, conflictResult.Value?.GetType().GetProperty("message")?.GetValue(conflictResult.Value));
        }

        [Fact]
        public void Controller_RequiresAuthenticatedUsersByDefault()
        {
            object attribute = Assert.Single(
                typeof(AccountController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));

            _ = Assert.IsType<AuthorizeAttribute>(attribute);
        }

        [Theory]
        [InlineData(nameof(AccountController.RegisterAsync))]
        [InlineData(nameof(AccountController.LoginAsync))]
        public void AnonymousAccountEndpoints_AllowAnonymousUsers(string actionName)
        {
            System.Reflection.MethodInfo method = typeof(AccountController).GetMethods()
                .Single(method => method.Name == actionName);

            object attribute = Assert.Single(
                method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));

            _ = Assert.IsType<AllowAnonymousAttribute>(attribute);
        }

        private sealed class CapturingAccountAuthenticationService : IAccountAuthenticationService
        {
            public string? CapturedRegisterEmail { get; private set; }

            public string? CapturedRegisterPassword { get; private set; }

            public string? CapturedRegisterName { get; private set; }

            public AccountRegistrationResult RegisterResult { get; init; } =
                AccountRegistrationResult.Failure("Unexpected call.");

            public Task<User?> AuthenticateAsync(
                string email,
                string password,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult<User?>(null);
            }

            public Task<AccountRegistrationResult> RegisterAsync(
                string email,
                string password,
                string name,
                CancellationToken cancellationToken = default)
            {
                CapturedRegisterEmail = email;
                CapturedRegisterPassword = password;
                CapturedRegisterName = name;
                return Task.FromResult(RegisterResult);
            }
        }
    }
}
