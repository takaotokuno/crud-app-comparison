using AspNetNextApp.Api.Contracts.Auth;
using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Tests.Controllers.Support;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class AuthControllerTests
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
            CapturingAuthService service = new()
            {
                RegisterResult = AccountRegistrationResult.Success(user),
            };
            AuthController controller = new(service, new FakeTokenService());
            RegisterRequest request = new("user@example.com", "password123", "New User");

            ActionResult<AccountUserResponse> actionResult = await controller.RegisterAsync(request, CancellationToken.None);

            CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            Assert.Equal(nameof(ProfileController.GetMe), createdResult.ActionName);
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
            CapturingAuthService service = new()
            {
                RegisterResult = AccountRegistrationResult.Failure(error),
            };
            AuthController controller = new(service, new FakeTokenService());
            RegisterRequest request = new("user@example.com", "password123", "New User");

            ActionResult<AccountUserResponse> actionResult = await controller.RegisterAsync(request, CancellationToken.None);

            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
            Assert.Equal(error, conflictResult.Value?.GetType().GetProperty("message")?.GetValue(conflictResult.Value));
        }

        [Fact]
        public void AuthController_RequiresAuthenticatedUsersByDefault()
        {
            Assert.Contains(
                typeof(AuthController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true),
                attribute => attribute is AuthorizeAttribute);
        }

        [Theory]
        [InlineData(nameof(AuthController.RegisterAsync))]
        [InlineData(nameof(AuthController.LoginAsync))]
        [InlineData(nameof(AuthController.RequestPasswordResetAsync))]
        [InlineData(nameof(AuthController.ResetPasswordAsync))]
        public void AnonymousAuthEndpoints_AllowAnonymousUsers(string actionName)
        {
            System.Reflection.MethodInfo method = typeof(AuthController).GetMethods()
                .Single(method => method.Name == actionName);

            object attribute = Assert.Single(
                method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));

            _ = Assert.IsType<AllowAnonymousAttribute>(attribute);
        }

        [Theory]
        [InlineData(nameof(AuthController.LoginAsync), "login")]
        [InlineData(nameof(AuthController.LogoutAsync), "logout")]
        public void AuthEndpoints_UseStandardRouteTemplates(string actionName, string routeTemplate)
        {
            System.Reflection.MethodInfo method = typeof(AuthController).GetMethods()
                .Single(method => method.Name == actionName);

            HttpMethodAttribute attribute = Assert.Single(
                method.GetCustomAttributes(typeof(HttpMethodAttribute), inherit: true).Cast<HttpMethodAttribute>());

            Assert.Equal(routeTemplate, attribute.Template);
        }
    }
}
