using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Auth
{
    public sealed class AuthService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IUserService userService) : IAuthService
    {
        public async Task<User?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

            return user is null || !VerifyPassword(user, password) ? null : user;
        }

        public async Task<AccountRegistrationResult> RegisterAsync(
            string email,
            string password,
            string name,
            CancellationToken cancellationToken = default)
        {
            AccountResult<User> result = await userService.CreateUserAsync(email, password, name, cancellationToken);
            return result.IsSuccess && result.Value is not null
                ? AccountRegistrationResult.Success(result.Value)
                : AccountRegistrationResult.Failure(result.Error ?? "Registration failed.");
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public bool VerifyPassword(User user, string password)
        {
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }

        public Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
        {
            return dbContext.Users.AsNoTracking().AnyAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<AccountResult<bool>> ResetPasswordAsync(string email, string newPassword, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
            if (user is null)
            {
                return AccountResult<bool>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<bool>.Success(true);
        }
    }
}
