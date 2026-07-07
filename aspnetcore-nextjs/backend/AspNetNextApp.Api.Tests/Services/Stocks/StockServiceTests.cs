using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Stocks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetNextApp.Api.Tests.Services.Stocks
{
    public sealed class StockServiceTests
    {
        [Fact]
        public async Task ListAsync_ReturnsStocksWithProductInformation()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 3, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            StockService service = new(dbContext);

            StockResult<Contracts.Stocks.StockListResponse> result = await service.ListAsync(
                new ListStocksQuery(null, true, null, null, 1, 20),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Contracts.Stocks.StockSummaryResponse item = Assert.Single(result.Value.Items);
            Assert.Equal(product.Id, item.ProductId);
            Assert.Equal("SKU-001", item.ProductSku);
            Assert.Equal("Coffee Beans", item.ProductName);
            Assert.Equal(3, item.Quantity);
            Assert.True(item.IsLowStock);
        }

        [Fact]
        public async Task UpdateAsync_WhenQuantityChangesPersistsStockAndTransaction()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 10, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            Guid stockId = product.Stock!.Id;
            StockService service = new(dbContext);

            StockResult<Contracts.Stocks.StockDetailResponse> result = await service.UpdateAsync(
                new UpdateStockCommand(stockId, 7, 4, "Inventory count"),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(7, result.Value.Quantity);
            Assert.Equal(4, result.Value.SafetyStock);
            StockTransaction transaction = Assert.Single(await dbContext.StockTransactions.ToListAsync());
            Assert.Equal(-3, transaction.QuantityDelta);
            Assert.Equal(7, transaction.QuantityAfter);
            Assert.Equal("Inventory count", transaction.Reason);
        }

        [Fact]
        public async Task UpdateAsync_WhenQuantityIsNegativeReturnsValidationFailure()
        {
            await using AppDbContext dbContext = CreateDbContext();
            StockService service = new(dbContext);

            StockResult<Contracts.Stocks.StockDetailResponse> result = await service.UpdateAsync(
                new UpdateStockCommand(Guid.NewGuid(), -1, 0, null),
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(StockErrorType.Validation, result.ErrorType);
            Assert.Equal("Quantity must be zero or greater.", result.Error);
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
