using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;

namespace AspNetNextApp.Api.Tests.Controllers.Support
{
    internal sealed class CapturingAccountAuthenticationService : IAccountAuthenticationService
    {
        public string? CapturedRegisterEmail { get; private set; }

        public string? CapturedRegisterPassword { get; private set; }

        public string? CapturedRegisterName { get; private set; }

        public AccountRegistrationResult RegisterResult { get; init; } =
            AccountRegistrationResult.Failure("Unexpected call.");

        public Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task<AccountRegistrationResult> RegisterAsync(string email, string password, string name, CancellationToken cancellationToken = default)
        {
            CapturedRegisterEmail = email;
            CapturedRegisterPassword = password;
            CapturedRegisterName = name;
            return Task.FromResult(RegisterResult);
        }

        public Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<AccountResult<bool>> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<bool>.Success(true));

        public Task<AccountResult<AccountUserListResponse>> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<AccountUserListResponse>.Success(new AccountUserListResponse([], page, pageSize, 0)));

        public Task<AccountResult<AccountUserResponse>> GetUserAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<AccountUserResponse>.Failure("Not found.", AccountErrorType.NotFound));

        public Task<AccountResult<AccountUserResponse>> UpdateUserAsync(Guid id, string email, string name, UserRole role, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<AccountUserResponse>.Failure("Not found.", AccountErrorType.NotFound));

        public Task<AccountResult<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<bool>.Success(true));

        public Task<AccountResult<AccountUserResponse>> ChangeUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<AccountUserResponse>.Failure("Not found.", AccountErrorType.NotFound));

        public Task<AccountResult<AccountUserResponse>> UpdateProfileAsync(Guid id, string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<AccountUserResponse>.Failure("Not found.", AccountErrorType.NotFound));

        public Task<AccountResult<bool>> ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken cancellationToken = default) =>
            Task.FromResult(AccountResult<bool>.Success(true));
    }
}
