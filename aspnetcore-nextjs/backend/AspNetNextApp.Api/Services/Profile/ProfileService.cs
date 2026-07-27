using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Profile
{
    public sealed class ProfileService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IAuthService authService) : IProfileService
    {
        public async Task<AccountResult<AccountUserResponse>> GetProfileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            User? user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            return user is null
                ? AccountResult<AccountUserResponse>.Failure("User was not found.", AccountErrorType.NotFound)
                : AccountResult<AccountUserResponse>.Success(ToResponse(user));
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

            if (!authService.VerifyPassword(user, currentPassword))
            {
                return AccountResult<bool>.Failure("Current password is invalid.", AccountErrorType.Unauthorized);
            }

            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            _ = await dbContext.SaveChangesAsync(cancellationToken);
            return AccountResult<bool>.Success(true);
        }

        private static AccountUserResponse ToResponse(User user)
        {
            return new(user.Id, user.Email, user.Name, user.Role, user.CreatedAt, user.UpdatedAt);
        }
    }
}
