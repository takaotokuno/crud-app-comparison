using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Services.Accounts
{
    public interface IAccountAuthenticationService
    {
        Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);

        Task<AccountRegistrationResult> RegisterAsync(
            string email,
            string password,
            string name,
            CancellationToken cancellationToken = default);

        Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

        Task<AccountResult<bool>> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserListResponse>> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> GetUserAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> UpdateUserAsync(Guid id, string email, string name, UserRole role, CancellationToken cancellationToken = default);

        Task<AccountResult<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> ChangeUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> UpdateProfileAsync(Guid id, string name, CancellationToken cancellationToken = default);

        Task<AccountResult<bool>> ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    }
}
