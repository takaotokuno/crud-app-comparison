using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Accounts
{
    public sealed class AccountAuthenticationService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher) : IAccountAuthenticationService
    {
        private const int MaxPageSize = 100;

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
            bool emailExists = await IsEmailInUseAsync(email, excludedUserId: null, cancellationToken);

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

        public async Task<AccountResult<AccountUserListResponse>> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            int normalizedPage = Math.Max(page, 1);
            int normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            IQueryable<User> users = dbContext.Users.AsNoTracking().OrderBy(user => user.Email);
            int totalCount = await users.CountAsync(cancellationToken);
            List<AccountUserResponse> items = await users
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(user => ToResponse(user))
                .ToListAsync(cancellationToken);

            return AccountResult<AccountUserListResponse>.Success(new AccountUserListResponse(items, normalizedPage, normalizedPageSize, totalCount));
        }

        public async Task<AccountResult<AccountUserResponse>> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            return user is null
                ? AccountResult<AccountUserResponse>.Failure("User was not found.", AccountErrorType.NotFound)
                : AccountResult<AccountUserResponse>.Success(ToResponse(user));
        }

        public async Task<AccountResult<AccountUserResponse>> UpdateUserAsync(
            Guid id,
            string email,
            string name,
            UserRole role,
            CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (user is null)
            {
                return AccountResult<AccountUserResponse>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            if (await IsEmailInUseAsync(email, id, cancellationToken))
            {
                return AccountResult<AccountUserResponse>.Failure("Email is already registered.", AccountErrorType.Conflict);
            }

            user.Email = email;
            user.Name = name;
            user.Role = role;
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<AccountUserResponse>.Success(ToResponse(user));
        }

        public async Task<AccountResult<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.FindAsync([id], cancellationToken);
            if (user is null)
            {
                return AccountResult<bool>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            _ = dbContext.Users.Remove(user);
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<bool>.Success(true);
        }

        public Task<AccountResult<AccountUserResponse>> ChangeUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken = default)
        {
            return UpdateRoleAsync(id, role, cancellationToken);
        }

        public async Task<AccountResult<AccountUserResponse>> UpdateProfileAsync(Guid id, string name, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (user is null)
            {
                return AccountResult<AccountUserResponse>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            user.Name = name;
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<AccountUserResponse>.Success(ToResponse(user));
        }

        public async Task<AccountResult<bool>> ChangePasswordAsync(Guid id, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (user is null)
            {
                return AccountResult<bool>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            if (!IsValidPassword(user, currentPassword))
            {
                return AccountResult<bool>.Failure("Current password is invalid.", AccountErrorType.Unauthorized);
            }

            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<bool>.Success(true);
        }

        private async Task<AccountResult<AccountUserResponse>> UpdateRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken)
        {
            User? user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (user is null)
            {
                return AccountResult<AccountUserResponse>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            user.Role = role;
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<AccountUserResponse>.Success(ToResponse(user));
        }

        private Task<bool> IsEmailInUseAsync(string email, Guid? excludedUserId, CancellationToken cancellationToken)
        {
            return dbContext.Users.AnyAsync(
                user => user.Email == email && (!excludedUserId.HasValue || user.Id != excludedUserId.Value),
                cancellationToken);
        }

        private bool IsValidPassword(User user, string password)
        {
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
        }

        private static AccountUserResponse ToResponse(User user)
        {
            return new(user.Id, user.Email, user.Name, user.Role);
        }
    }
}
