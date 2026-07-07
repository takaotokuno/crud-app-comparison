using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Contracts.Account;

public sealed record LoginRequest(
    [property: Required]
    [property: EmailAddress]
    [property: MaxLength(255)]
    string Email,

    [property: Required]
    [property: MinLength(1)]
    [property: MaxLength(200)]
    string Password);
