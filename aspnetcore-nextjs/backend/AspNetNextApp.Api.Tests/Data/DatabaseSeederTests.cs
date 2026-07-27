using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetNextApp.Api.Tests.Data
{
    public sealed class DatabaseSeederTests
    {
        [Fact]
        public async Task SeedAsync_CreatesRequiredUsersProductsAndStocks()
        {
            await using AppDbContext dbContext = CreateDbContext();
            PasswordHasher<User> passwordHasher = new();

            await DatabaseSeeder.SeedAsync(dbContext, passwordHasher);

            User admin = await dbContext.Users.SingleAsync(user => user.Email == "admin@example.com");
            Assert.Equal("管理者", admin.Name);
            Assert.Equal(UserRole.Admin, admin.Role);
            Assert.Equal(
                PasswordVerificationResult.Success,
                passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, "password"));
            Assert.DoesNotContain("password", admin.PasswordHash, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(3, await dbContext.Users.CountAsync());
            Assert.Equal(5, await dbContext.Products.CountAsync());
            Assert.Equal(5, await dbContext.Stocks.CountAsync());

            Product lowStockProduct = await dbContext.Products
                .Include(product => product.Stock)
                .SingleAsync(product => product.Sku == "SKU-002");
            Assert.Equal("ワイヤレスマウス", lowStockProduct.Name);
            Assert.Equal(2, lowStockProduct.Stock!.Quantity);
            Assert.Equal(5, lowStockProduct.Stock.SafetyStock);
        }

        [Fact]
        public async Task SeedAsync_WhenRunTwice_DoesNotCreateDuplicates()
        {
            await using AppDbContext dbContext = CreateDbContext();
            PasswordHasher<User> passwordHasher = new();

            await DatabaseSeeder.SeedAsync(dbContext, passwordHasher);
            await DatabaseSeeder.SeedAsync(dbContext, passwordHasher);

            Assert.Equal(3, await dbContext.Users.CountAsync());
            Assert.Equal(5, await dbContext.Products.CountAsync());
            Assert.Equal(5, await dbContext.Stocks.CountAsync());
        }

        private static AppDbContext CreateDbContext()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
