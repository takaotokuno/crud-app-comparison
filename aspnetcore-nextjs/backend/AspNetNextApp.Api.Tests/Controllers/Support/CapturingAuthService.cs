using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Auth;

namespace AspNetNextApp.Api.Tests.Controllers.Support
{
    internal sealed class CapturingAuthService : IAuthService
    {
        public string? CapturedRegisterEmail { get; private set; }

        public string? CapturedRegisterPassword { get; private set; }

        public string? CapturedRegisterName { get; private set; }

        public AccountRegistrationResult RegisterResult { get; init; } =
            AccountRegistrationResult.Failure("Unexpected call.");

        public Task<User?> LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task<AccountRegistrationResult> RegisterAsync(string email, string password, string name, CancellationToken cancellationToken = default)
        {
            CapturedRegisterEmail = email;
            CapturedRegisterPassword = password;
            CapturedRegisterName = name;
            return Task.FromResult(RegisterResult);
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public bool VerifyPassword(User user, string password) => false;

        public Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<AccountResult<bool>> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<bool>.Success(true));
    }
}
