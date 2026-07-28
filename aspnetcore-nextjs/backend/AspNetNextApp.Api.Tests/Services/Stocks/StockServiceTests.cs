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
        public async Task AdjustAsync_PersistsStockAndTransaction()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 10, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            Guid stockId = product.Stock!.Id;
            StockService service = new(dbContext);

            Guid userId = Guid.NewGuid();
            StockResult<Contracts.Stocks.StockDetailResponse> result = await service.AdjustAsync(
                new AdjustStockCommand(stockId, 7, 10, "Inventory count", userId),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(7, result.Value.Quantity);
            Assert.Equal(5, result.Value.SafetyStock);
            StockTransaction transaction = Assert.Single(await dbContext.StockTransactions.ToListAsync());
            Assert.Equal(-3, transaction.QuantityDelta);
            Assert.Equal(7, transaction.QuantityAfter);
            Assert.Equal("Inventory count", transaction.Reason);
            Assert.Equal(userId, transaction.CreatedById);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesSafetyStockWithoutCreatingTransaction()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 10, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            StockService service = new(dbContext);

            StockResult<Contracts.Stocks.StockDetailResponse> result = await service.UpdateAsync(
                new UpdateStockCommand(product.Stock!.Id, 4),
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(10, result.Value.Quantity);
            Assert.Equal(4, result.Value.SafetyStock);
            Assert.Empty(await dbContext.StockTransactions.ToListAsync());
        }

        [Fact]
        public async Task AdjustAsync_WhenQuantityChangedReturnsConflict()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create("SKU-001", "Coffee Beans", null, "Beverage", 1200, ProductStatus.Active, 10, 5);
            _ = dbContext.Products.Add(product);
            _ = await dbContext.SaveChangesAsync();
            StockService service = new(dbContext);

            StockResult<Contracts.Stocks.StockDetailResponse> result = await service.AdjustAsync(
                new AdjustStockCommand(product.Stock!.Id, 7, 9, "Inventory count", null),
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(StockErrorType.Conflict, result.ErrorType);
            Assert.Empty(await dbContext.StockTransactions.ToListAsync());
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
