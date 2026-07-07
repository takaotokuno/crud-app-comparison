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

        private bool IsValidPassword(User user, string password)
        {
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
