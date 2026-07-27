using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Data
{
    public static class DatabaseSeeder
    {
        private const string InitialPassword = "password";

        public static async Task SeedAsync(
            AppDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            CancellationToken cancellationToken = default)
        {
            await SeedUsersAsync(dbContext, passwordHasher, cancellationToken);
            await SeedProductsAsync(dbContext, cancellationToken);
        }

        private static async Task SeedUsersAsync(
            AppDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            CancellationToken cancellationToken)
        {
            HashSet<string> existingEmails = await dbContext.Users
                .Select(user => user.Email)
                .ToHashSetAsync(cancellationToken);

            (string Email, string Name, UserRole Role)[] users =
            [
                ("admin@example.com", "管理者", UserRole.Admin),
                ("staff@example.com", "在庫担当者", UserRole.Staff),
                ("viewer@example.com", "閲覧者", UserRole.Viewer),
            ];

            foreach ((string email, string name, UserRole role) in users)
            {
                if (existingEmails.Contains(email))
                {
                    continue;
                }

                User user = new()
                {
                    Email = email,
                    Name = name,
                    Role = role,
                };
                user.PasswordHash = passwordHasher.HashPassword(user, InitialPassword);
                _ = dbContext.Users.Add(user);
            }

            _ = await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedProductsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
        {
            HashSet<string> existingSkus = await dbContext.Products
                .Select(product => product.Sku)
                .ToHashSetAsync(cancellationToken);

            Product[] products =
            [
                Product.Create("SKU-001", "ノートPC 14インチ", null, "PC", 120000, ProductStatus.Active, 8, 3),
                Product.Create("SKU-002", "ワイヤレスマウス", null, "周辺機器", 2500, ProductStatus.Active, 2, 5),
                Product.Create("SKU-003", "USB-Cハブ", null, "周辺機器", 4800, ProductStatus.Active, 15, 5),
                Product.Create("SKU-004", "27インチモニター", null, "ディスプレイ", 32000, ProductStatus.Inactive, 0, 2),
                Product.Create("SKU-005", "旧型キーボード", null, "周辺機器", 1800, ProductStatus.Discontinued, 1, 1),
            ];

            foreach (Product product in products)
            {
                if (!existingSkus.Contains(product.Sku))
                {
                    _ = dbContext.Products.Add(product);
                }
            }

            _ = await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
