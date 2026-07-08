using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Users
{
    public sealed class UserService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher) : IUserService
    {
        private const int MaxPageSize = 100;

        public async Task<AccountResult<User>> CreateUserAsync(
            string email,
            string password,
            string name,
            CancellationToken cancellationToken = default)
        {
            if (await IsEmailInUseAsync(email, excludedUserId: null, cancellationToken))
            {
                return AccountResult<User>.Failure("Email is already registered.", AccountErrorType.Conflict);
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
                return AccountResult<User>.Failure("Email is already registered.", AccountErrorType.Conflict);
            }

            return AccountResult<User>.Success(user);
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

        public async Task<AccountResult<AccountUserResponse>> ChangeUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken = default)
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

        public Task<bool> IsEmailInUseAsync(string email, Guid? excludedUserId, CancellationToken cancellationToken = default)
        {
            return dbContext.Users.AnyAsync(
                user => user.Email == email && (!excludedUserId.HasValue || user.Id != excludedUserId.Value),
                cancellationToken);
        }

        private static AccountUserResponse ToResponse(User user)
        {
            return new(user.Id, user.Email, user.Name, user.Role);
        }
    }
}
