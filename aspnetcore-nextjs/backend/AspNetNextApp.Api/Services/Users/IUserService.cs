using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;

namespace AspNetNextApp.Api.Services.Users
{
    public interface IUserService
    {
        Task<AccountResult<User>> CreateUserAsync(string email, string password, string name, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserListResponse>> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> GetUserAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> UpdateUserAsync(Guid id, string email, string name, UserRole role, CancellationToken cancellationToken = default);

        Task<AccountResult<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AccountResult<AccountUserResponse>> ChangeUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken = default);

        Task<bool> IsEmailInUseAsync(string email, Guid? excludedUserId, CancellationToken cancellationToken = default);
    }
}
