using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Contracts.Auth
{
    public sealed record LoginRequest(
        [param: Required]
        [param: EmailAddress]
        [param: MaxLength(255)]
        string Email,

        [param: Required]
        [param: MinLength(1)]
        [param: MaxLength(200)]
        string Password);

    public sealed record RegisterRequest(
        [param: Required]
        [param: EmailAddress]
        [param: MaxLength(255)]
        string Email,

        [param: Required]
        [param: MinLength(8)]
        [param: MaxLength(200)]
        string Password,

        [param: Required]
        [param: MaxLength(100)]
        string Name);

    public sealed record RequestPasswordResetRequest(
        [param: Required]
        [param: EmailAddress]
        [param: MaxLength(255)]
        string Email);

    public sealed record ResetPasswordRequest(
        [param: Required]
        [param: EmailAddress]
        [param: MaxLength(255)]
        string Email,

        [param: Required]
        [param: MinLength(8)]
        [param: MaxLength(200)]
        string NewPassword);
}
