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
            UserRole role,
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
                Role = role,
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

        public async Task<AccountResult<AccountUserListResponse>> ListUsersAsync(ListUsersQuery query, CancellationToken cancellationToken = default)
        {
            int normalizedPage = Math.Max(query.Page, 1);
            int normalizedPageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            IQueryable<User> users = dbContext.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                string keyword = query.Query.Trim();
                users = users.Where(user => user.Email.Contains(keyword) || user.Name.Contains(keyword));
            }

            if (query.Role.HasValue)
            {
                users = users.Where(user => user.Role == query.Role.Value);
            }

            int totalCount = await users.CountAsync(cancellationToken);
            List<AccountUserResponse> items = await ApplySort(users, query.SortBy, query.SortDirection)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(user => ToResponse(user))
                .ToListAsync(cancellationToken);

            return AccountResult<AccountUserListResponse>.Success(new AccountUserListResponse(items, normalizedPage, normalizedPageSize, totalCount));
        }

        private static IOrderedQueryable<User> ApplySort(IQueryable<User> users, string? sortBy, string? sortDirection)
        {
            bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "email" => descending ? users.OrderByDescending(user => user.Email) : users.OrderBy(user => user.Email),
                "name" => descending ? users.OrderByDescending(user => user.Name) : users.OrderBy(user => user.Name),
                "role" => descending ? users.OrderByDescending(user => user.Role) : users.OrderBy(user => user.Role),
                "updated_at" => descending ? users.OrderByDescending(user => user.UpdatedAt) : users.OrderBy(user => user.UpdatedAt),
                "created_at" or _ => descending || string.IsNullOrWhiteSpace(sortDirection)
                    ? users.OrderByDescending(user => user.CreatedAt)
                    : users.OrderBy(user => user.CreatedAt),
            };
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

            if (user.Role == UserRole.Admin
                && role != UserRole.Admin
                && !await HasAnotherAdminAsync(id, cancellationToken))
            {
                return AccountResult<AccountUserResponse>.Failure("The last admin user cannot be demoted.", AccountErrorType.Conflict);
            }

            user.Email = email;
            user.Name = name;
            user.Role = role;
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<AccountUserResponse>.Success(ToResponse(user));
        }

        public async Task<AccountResult<bool>> DeleteUserAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.FindAsync([id], cancellationToken);
            if (user is null)
            {
                return AccountResult<bool>.Failure("User was not found.", AccountErrorType.NotFound);
            }

            if (id == currentUserId)
            {
                return AccountResult<bool>.Failure("You cannot delete your own user account.", AccountErrorType.Conflict);
            }

            if (user.Role == UserRole.Admin && !await HasAnotherAdminAsync(id, cancellationToken))
            {
                return AccountResult<bool>.Failure("The last admin user cannot be deleted.", AccountErrorType.Conflict);
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

            if (user.Role == UserRole.Admin
                && role != UserRole.Admin
                && !await HasAnotherAdminAsync(id, cancellationToken))
            {
                return AccountResult<AccountUserResponse>.Failure("The last admin user cannot be demoted.", AccountErrorType.Conflict);
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

        private Task<bool> HasAnotherAdminAsync(Guid excludedUserId, CancellationToken cancellationToken)
        {
            return dbContext.Users.AnyAsync(
                user => user.Id != excludedUserId && user.Role == UserRole.Admin,
                cancellationToken);
        }

        private static AccountUserResponse ToResponse(User user)
        {
            return new(user.Id, user.Email, user.Name, user.Role, user.CreatedAt, user.UpdatedAt);
        }
    }
}
