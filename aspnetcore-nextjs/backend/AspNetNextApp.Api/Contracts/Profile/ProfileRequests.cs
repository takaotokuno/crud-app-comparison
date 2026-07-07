using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Contracts.Profile
{
    public sealed record UpdateProfileRequest(
        [property: Required]
        [property: MaxLength(100)]
        string Name);

    public sealed record ChangePasswordRequest(
        [property: Required]
        [property: MinLength(1)]
        [property: MaxLength(200)]
        string CurrentPassword,

        [property: Required]
        [property: MinLength(8)]
        [property: MaxLength(200)]
        string NewPassword);
}
