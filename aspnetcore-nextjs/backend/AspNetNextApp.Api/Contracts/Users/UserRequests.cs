using System.ComponentModel.DataAnnotations;

using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Contracts.Users
{
    public sealed record UpdateUserRequest(
        [property: Required]
        [property: EmailAddress]
        [property: MaxLength(255)]
        string Email,

        [property: Required]
        [property: MaxLength(100)]
        string Name,

        UserRole Role);

    public sealed record ChangeUserRoleRequest(UserRole Role);
}
