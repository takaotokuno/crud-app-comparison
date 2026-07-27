using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Services.Accounts;

namespace AspNetNextApp.Api.Services.Profile
{
    public interface IProfileService
    {
        Task<AccountResult<AccountUserResponse>> GetProfileAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> UpdateProfileAsync(Guid id, string name, CancellationToken cancellationToken = default);

        Task<AccountResult<bool>> ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    }
}
