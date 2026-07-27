using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Contracts.Users
{
    public sealed record AccountUserResponse(
        Guid Id,
        string Email,
        string Name,
        UserRole Role,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    public sealed record AccountUserListResponse(
        IReadOnlyCollection<AccountUserResponse> Items,
        int Page,
        int PageSize,
        int TotalCount);
}
