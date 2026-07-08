using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;

namespace AspNetNextApp.Api.Services.Auth
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

        Task<AccountRegistrationResult> RegisterAsync(string email, string password, string name, CancellationToken cancellationToken = default);

        Task LogoutAsync(CancellationToken cancellationToken = default);

        bool VerifyPassword(User user, string password);

        Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

        Task<AccountResult<bool>> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default);
    }
}
