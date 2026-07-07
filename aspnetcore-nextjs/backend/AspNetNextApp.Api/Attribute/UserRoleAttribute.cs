using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AspNetNextApp.Api.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class UserRoleAttribute : AuthorizeAttribute
{
    public UserRoleAttribute(UserRole role, params UserRole[] additionalRoles)
    {
        UserRoles = [role, ..additionalRoles];
        Roles = string.Join(',', UserRoles.Select(userRole => userRole.ToString()));
    }

    public IReadOnlyList<UserRole> UserRoles { get; }
}
