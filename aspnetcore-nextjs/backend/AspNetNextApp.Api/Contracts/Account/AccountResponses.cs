using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Contracts.Account;

public sealed record AccountUserResponse(
    Guid Id,
    string Email,
    string Name,
    UserRole Role);
