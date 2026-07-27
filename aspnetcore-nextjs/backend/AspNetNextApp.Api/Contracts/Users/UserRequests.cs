using System.ComponentModel.DataAnnotations;

using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Contracts.Users
{
    public sealed class ListUsersRequest
    {
        [FromQuery(Name = "q")]
        [MaxLength(255)]
        public string? Query { get; init; }

        [FromQuery]
        public UserRole? Role { get; init; }

        [FromQuery(Name = "sort_by")]
        [MaxLength(32)]
        public string? SortBy { get; init; }

        [FromQuery(Name = "sort_direction")]
        [MaxLength(4)]
        public string? SortDirection { get; init; }

        [FromQuery]
        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;

        [FromQuery(Name = "page_size")]
        [Range(1, 100)]
        public int PageSize { get; init; } = 20;
    }

    public sealed record CreateUserRequest(
        [property: Required]
        [property: EmailAddress]
        [property: MaxLength(255)]
        string Email,

        [property: Required]
        [property: MinLength(8)]
        [property: MaxLength(200)]
        string Password,

        [property: Required]
        [property: MaxLength(100)]
        string Name,

        UserRole Role);

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
