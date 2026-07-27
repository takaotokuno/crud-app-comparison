using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Services.Users
{
    public sealed record ListUsersQuery(
        string? Query,
        UserRole? Role,
        string? SortBy,
        string? SortDirection,
        int Page,
        int PageSize);
}
