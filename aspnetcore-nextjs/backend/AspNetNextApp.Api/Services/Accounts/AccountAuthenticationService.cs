using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Accounts
{
    public sealed class AccountAuthenticationService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher) : IAccountAuthenticationService
    {
        public async Task<User?> AuthenticateAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

            return user is null || !IsValidPassword(user, password) ? null : user;
        }

        public async Task<AccountRegistrationResult> RegisterAsync(
            string email,
            string password,
            string name,
            CancellationToken cancellationToken = default)
        {
            bool emailExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(candidate => candidate.Email == email, cancellationToken);

            if (emailExists)
            {
                return AccountRegistrationResult.Failure("Email is already registered.");
            }

            User user = new()
            {
                Email = email,
                Name = name,
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            _ = dbContext.Users.Add(user);

            try
            {
                _ = await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return AccountRegistrationResult.Failure("Email is already registered.");
            }

            return AccountRegistrationResult.Success(user);
        }

        private bool IsValidPassword(User user, string password)
        {
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
