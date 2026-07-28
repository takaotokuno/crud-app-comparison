using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Contracts.Profile
{
    public sealed record UpdateProfileRequest(
        [param: Required]
        [param: MaxLength(100)]
        string Name);

    public sealed record ChangePasswordRequest(
        [param: Required]
        [param: MinLength(1)]
        [param: MaxLength(200)]
        string CurrentPassword,

        [param: Required]
        [param: MinLength(8)]
        [param: MaxLength(200)]
        string NewPassword);
}
