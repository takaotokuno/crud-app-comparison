using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Contracts.Auth
{
    public sealed record LoginRequest(
        [property: Required]
        [property: EmailAddress]
        [property: MaxLength(255)]
        string Email,

        [property: Required]
        [property: MinLength(1)]
        [property: MaxLength(200)]
        string Password);

    public sealed record RegisterRequest(
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
        string Name);

    public sealed record RequestPasswordResetRequest(
        [property: Required]
        [property: EmailAddress]
        [property: MaxLength(255)]
        string Email);

    public sealed record ResetPasswordRequest(
        [property: Required]
        [property: EmailAddress]
        [property: MaxLength(255)]
        string Email,

        [property: Required]
        [property: MinLength(8)]
        [property: MaxLength(200)]
        string NewPassword);
}
