using AspNetNextApp.Api.Contracts.Users;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Accounts;
using AspNetNextApp.Api.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetNextApp.Api.Tests.Services.Users
{
    public sealed class UserServiceTests
    {
        [Fact]
        public async Task DeleteUserAsync_WhenDeletingCurrentUserReturnsConflict()
        {
            await using AppDbContext dbContext = CreateDbContext();
            User admin = AddUser(dbContext, UserRole.Admin);
            _ = await dbContext.SaveChangesAsync();

            AccountResult<bool> result = await CreateService(dbContext).DeleteUserAsync(admin.Id, admin.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal(AccountErrorType.Conflict, result.ErrorType);
            Assert.NotNull(await dbContext.Users.FindAsync(admin.Id));
        }

        [Fact]
        public async Task DeleteUserAsync_WhenTargetIsLastAdminReturnsConflict()
        {
            await using AppDbContext dbContext = CreateDbContext();
            User admin = AddUser(dbContext, UserRole.Admin);
            User currentUser = AddUser(dbContext, UserRole.Staff);
            _ = await dbContext.SaveChangesAsync();

            AccountResult<bool> result = await CreateService(dbContext).DeleteUserAsync(admin.Id, currentUser.Id);

            Assert.False(result.IsSuccess);
            Assert.Equal(AccountErrorType.Conflict, result.ErrorType);
            Assert.NotNull(await dbContext.Users.FindAsync(admin.Id));
        }

        [Fact]
        public async Task DeleteUserAsync_WhenAnotherAdminExistsDeletesTarget()
        {
            await using AppDbContext dbContext = CreateDbContext();
            User target = AddUser(dbContext, UserRole.Admin);
            User currentUser = AddUser(dbContext, UserRole.Admin);
            _ = await dbContext.SaveChangesAsync();

            AccountResult<bool> result = await CreateService(dbContext).DeleteUserAsync(target.Id, currentUser.Id);

            Assert.True(result.IsSuccess);
            Assert.Null(await dbContext.Users.FindAsync(target.Id));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RoleUpdate_WhenDemotingLastAdminReturnsConflict(bool useRoleEndpoint)
        {
            await using AppDbContext dbContext = CreateDbContext();
            User admin = AddUser(dbContext, UserRole.Admin);
            _ = await dbContext.SaveChangesAsync();
            UserService service = CreateService(dbContext);

            AccountResult<AccountUserResponse> result = useRoleEndpoint
                ? await service.ChangeUserRoleAsync(admin.Id, UserRole.Staff)
                : await service.UpdateUserAsync(admin.Id, admin.Email, admin.Name, UserRole.Staff);

            Assert.False(result.IsSuccess);
            Assert.Equal(AccountErrorType.Conflict, result.ErrorType);
            Assert.Equal(UserRole.Admin, (await dbContext.Users.FindAsync(admin.Id))!.Role);
        }

        private static User AddUser(AppDbContext dbContext, UserRole role)
        {
            User user = new()
            {
                Email = $"{Guid.NewGuid():N}@example.com",
                Name = "Test User",
                PasswordHash = "hash",
                Role = role,
            };
            _ = dbContext.Users.Add(user);
            return user;
        }

        private static UserService CreateService(AppDbContext dbContext) => new(dbContext, new PasswordHasher<User>());

        private static AppDbContext CreateDbContext()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }
}
