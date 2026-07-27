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
        public async Task ListUsersAsync_AppliesSearchRoleSortAndPaging()
        {
            await using AppDbContext dbContext = CreateDbContext();
            _ = AddUser(dbContext, UserRole.Staff, "alice@example.com", "Alice", DateTimeOffset.Parse("2026-01-01"));
            _ = AddUser(dbContext, UserRole.Staff, "alicia@example.com", "Alicia", DateTimeOffset.Parse("2026-01-02"));
            _ = AddUser(dbContext, UserRole.Admin, "admin@example.com", "Alice Admin", DateTimeOffset.Parse("2026-01-03"));
            _ = AddUser(dbContext, UserRole.Staff, "bob@example.com", "Bob", DateTimeOffset.Parse("2026-01-04"));
            _ = await dbContext.SaveChangesAsync();

            AccountResult<AccountUserListResponse> result = await CreateService(dbContext).ListUsersAsync(
                new ListUsersQuery("ali", UserRole.Staff, "name", "desc", 2, 1));

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.TotalCount);
            Assert.Equal(2, result.Value.Page);
            Assert.Equal(1, result.Value.PageSize);
            Assert.Equal("Alice", Assert.Single(result.Value.Items).Name);
        }

        [Theory]
        [InlineData("email")]
        [InlineData("name")]
        [InlineData("role")]
        [InlineData("created_at")]
        [InlineData("updated_at")]
        public async Task ListUsersAsync_SortsEverySupportedFieldDescending(string sortBy)
        {
            await using AppDbContext dbContext = CreateDbContext();
            User first = AddUser(dbContext, UserRole.Admin, "a@example.com", "A", DateTimeOffset.Parse("2026-01-01"));
            first.UpdatedAt = DateTimeOffset.Parse("2026-02-01");
            User second = AddUser(dbContext, UserRole.Viewer, "z@example.com", "Z", DateTimeOffset.Parse("2026-01-02"));
            second.UpdatedAt = DateTimeOffset.Parse("2026-02-02");
            _ = await dbContext.SaveChangesAsync();

            AccountResult<AccountUserListResponse> result = await CreateService(dbContext).ListUsersAsync(
                new ListUsersQuery(null, null, sortBy, "desc", 1, 20));

            Assert.Equal(second.Id, result.Value!.Items.First().Id);
        }

        [Fact]
        public async Task ListUsersAsync_DefaultsToCreatedAtDescending()
        {
            await using AppDbContext dbContext = CreateDbContext();
            User older = AddUser(dbContext, UserRole.Staff, "z@example.com", "Z", DateTimeOffset.Parse("2026-01-01"));
            User newer = AddUser(dbContext, UserRole.Staff, "a@example.com", "A", DateTimeOffset.Parse("2026-01-02"));
            _ = await dbContext.SaveChangesAsync();

            AccountResult<AccountUserListResponse> result = await CreateService(dbContext).ListUsersAsync(
                new ListUsersQuery(null, null, null, null, 1, 20));

            Assert.Equal(newer.Id, result.Value!.Items.First().Id);
            Assert.Equal(older.Id, result.Value.Items.Last().Id);
        }

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

        private static User AddUser(
            AppDbContext dbContext,
            UserRole role,
            string? email = null,
            string name = "Test User",
            DateTimeOffset? createdAt = null)
        {
            User user = new()
            {
                Email = email ?? $"{Guid.NewGuid():N}@example.com",
                Name = name,
                PasswordHash = "hash",
                Role = role,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
                UpdatedAt = createdAt ?? DateTimeOffset.UtcNow,
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
