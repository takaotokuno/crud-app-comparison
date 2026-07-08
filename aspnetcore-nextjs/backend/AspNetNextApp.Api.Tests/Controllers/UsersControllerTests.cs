using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Enums;

using Microsoft.AspNetCore.Authorization;

using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class UsersControllerTests
    {
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

        [Theory]
        [InlineData(nameof(UsersController.ListAsync), null)]
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
    }
}
