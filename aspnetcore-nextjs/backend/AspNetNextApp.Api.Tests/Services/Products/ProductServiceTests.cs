using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Products;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetNextApp.Api.Tests.Services.Products
{
    public sealed class ProductServiceTests
    {
        [Fact]
        public async Task ListAsync_ReturnsEmptyPageUsingRequestedPaging()
        {
            await using AppDbContext dbContext = CreateDbContext();
            ProductService service = new(dbContext);
            ListProductsQuery query = new(
                Query: null,
                Status: null,
                Category: null,
                LowStock: null,
                SortBy: null,
                SortDirection: null,
                Page: 3,
                PageSize: 15);

            ProductResult<Contracts.Products.ProductListResponse> result = await service.ListAsync(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value.Items);
            Assert.Equal(3, result.Value.Page);
            Assert.Equal(15, result.Value.PageSize);
            Assert.Equal(0, result.Value.TotalCount);
        }

        [Fact]
        public async Task CreateAsync_WhenInputIsValidPersistsProductWithStock()
        {
            await using AppDbContext dbContext = CreateDbContext();
            ProductService service = new(dbContext);
            CreateProductCommand command = new(
                Sku: "SKU-001",
                Name: "Coffee Beans",
                Description: "Medium roast",
                Category: "Beverage",
                Price: 1200,
                Status: ProductStatus.Active,
                InitialQuantity: 25,
                SafetyStock: 5);

            ProductResult<Contracts.Products.ProductDetailResponse> result = await service.CreateAsync(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(command.Sku, result.Value.Sku);
            Assert.Equal(command.Name, result.Value.Name);
            Assert.Equal(command.Description, result.Value.Description);
            Assert.Equal(command.Category, result.Value.Category);
            Assert.Equal(command.Price, result.Value.Price);
            Assert.Equal(command.Status, result.Value.Status);
            Assert.Equal(command.InitialQuantity, result.Value.Quantity);
            Assert.Equal(command.SafetyStock, result.Value.SafetyStock);
            Assert.Equal(1, await dbContext.Products.CountAsync());
            Assert.Equal(1, await dbContext.Stocks.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_WhenProductExistsMarksProductDiscontinuedAndKeepsStockHistory()
        {
            await using AppDbContext dbContext = CreateDbContext();
            Product product = Product.Create(
                "SKU-DELETE",
                "Delete Target",
                null,
                "Test",
                1000,
                ProductStatus.Active,
                initialQuantity: 10,
                safetyStock: 2);
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            StockTransaction transaction = product.Stock!.ApplyTransaction(StockTransactionType.Inbound, 5, "restock");
            dbContext.StockTransactions.Add(transaction);
            await dbContext.SaveChangesAsync();
            Guid productId = product.Id;
            Guid stockId = product.Stock.Id;
            ProductService service = new(dbContext);

            ProductResult<bool> result = await service.DeleteAsync(new DeleteProductCommand(productId), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Product? savedProduct = await dbContext.Products.FindAsync(productId);
            Assert.NotNull(savedProduct);
            Assert.Equal(ProductStatus.Discontinued, savedProduct.Status);
            Assert.Equal(1, await dbContext.Stocks.CountAsync(stock => stock.Id == stockId));
            Assert.Equal(1, await dbContext.StockTransactions.CountAsync(transaction => transaction.ProductId == productId));
        }

        [Fact]
        public async Task GetAsync_WhenProductDoesNotExistReturnsNotFoundFailure()
        {
            await using AppDbContext dbContext = CreateDbContext();
            ProductService service = new(dbContext);

            ProductResult<Contracts.Products.ProductDetailResponse> result = await service.GetAsync(new GetProductQuery(Guid.NewGuid()), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(ProductErrorType.NotFound, result.ErrorType);
            Assert.Equal("Product was not found.", result.Error);
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
