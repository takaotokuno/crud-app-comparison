using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.StockTransactions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetNextApp.Api.Tests.Services.StockTransactions
{
    public sealed class StockTransactionServiceTests
    {
        [Fact]
        public async Task CreateAsync_WhenTypeIsAdjustment_ReturnsValidationErrorWithoutChangingStock()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 10, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            StockTransactionService service = new(dbContext);

            StockTransactionResult<Contracts.StockTransactions.StockTransactionResponse> result = await service.CreateAsync(
                new CreateStockTransactionCommand(product.Id, StockTransactionType.Adjustment, -3, "Inventory count", Guid.NewGuid()),
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(StockTransactionErrorType.Validation, result.ErrorType);
            Assert.Contains("Stock Adjust", result.Error);
            Assert.Equal(10, product.Stock!.Quantity);
            Assert.Empty(await dbContext.StockTransactions.ToListAsync());
        }

        [Theory]
        [InlineData(StockTransactionType.Inbound, 3, 13)]
        [InlineData(StockTransactionType.Outbound, -3, 7)]
        public async Task CreateAsync_WhenTypeIsGeneralTransaction_PersistsTransaction(
            StockTransactionType type,
            int quantityDelta,
            int expectedQuantity)
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 10, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            StockTransactionService service = new(dbContext);

            StockTransactionResult<Contracts.StockTransactions.StockTransactionResponse> result = await service.CreateAsync(
                new CreateStockTransactionCommand(product.Id, type, quantityDelta, null, Guid.NewGuid()),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedQuantity, result.Value!.QuantityAfter);
            Assert.Equal(expectedQuantity, product.Stock!.Quantity);
            Assert.Single(await dbContext.StockTransactions.ToListAsync());
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
