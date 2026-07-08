using AspNetNextApp.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class ProfileControllerTests
    {
        [Fact]
        public void ProfileController_RequiresAuthenticatedUsersByDefault()
        {
            Assert.Contains(
                typeof(ProfileController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true),
                attribute => attribute is AuthorizeAttribute);
        }

        [Theory]
        [InlineData(nameof(ProfileController.GetMe), "me")]
        [InlineData(nameof(ProfileController.UpdateProfileAsync), "me")]
        [InlineData(nameof(ProfileController.ChangePasswordAsync), "me/password")]
        public void ProfileEndpoints_UseStandardRouteTemplates(string actionName, string routeTemplate)
        {
            System.Reflection.MethodInfo method = typeof(ProfileController).GetMethods()
                .Single(method => method.Name == actionName);

            HttpMethodAttribute attribute = Assert.Single(
                method.GetCustomAttributes(typeof(HttpMethodAttribute), inherit: true).Cast<HttpMethodAttribute>());

            Assert.Equal(routeTemplate, attribute.Template);
        }
    }
}
