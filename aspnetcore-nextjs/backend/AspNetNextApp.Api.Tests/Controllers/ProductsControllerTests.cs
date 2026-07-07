using AspNetNextApp.Api.Contracts.Products;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Services.Products;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Tests.Controllers;

public sealed class ProductsControllerTests
{
    [Fact]
    public async Task ListAsync_ForwardsQueryParametersAndReturnsOk()
    {
        var response = new ProductListResponse([], 2, 10, 0);
        var service = new CapturingProductService
        {
            ListResult = ProductResult<ProductListResponse>.Success(response)
        };
        var controller = new ProductsController(service);

        var actionResult = await controller.ListAsync(
            query: "coffee",
            status: ProductStatus.Active,
            category: "Beverage",
            lowStock: true,
            sortBy: "name",
            sortDirection: "desc",
            page: 2,
            pageSize: 10);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(response, okResult.Value);
        Assert.NotNull(service.CapturedListQuery);
        Assert.Equal("coffee", service.CapturedListQuery.Query);
        Assert.Equal(ProductStatus.Active, service.CapturedListQuery.Status);
        Assert.Equal("Beverage", service.CapturedListQuery.Category);
        Assert.True(service.CapturedListQuery.LowStock);
        Assert.Equal("name", service.CapturedListQuery.SortBy);
        Assert.Equal("desc", service.CapturedListQuery.SortDirection);
        Assert.Equal(2, service.CapturedListQuery.Page);
        Assert.Equal(10, service.CapturedListQuery.PageSize);
    }

    [Fact]
    public async Task CreateAsync_WhenServiceSucceedsReturnsCreatedAtGetRoute()
    {
        var productId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var updatedAt = DateTimeOffset.UtcNow;
        var response = new ProductDetailResponse(
            productId,
            "SKU-001",
            "Coffee Beans",
            "Medium roast",
            "Beverage",
            1200,
            ProductStatus.Active,
            25,
            5,
            createdAt,
            updatedAt);
        var service = new CapturingProductService
        {
            CreateResult = ProductResult<ProductDetailResponse>.Success(response)
        };
        var controller = new ProductsController(service);
        var request = new CreateProductRequest(
            "SKU-001",
            "Coffee Beans",
            "Medium roast",
            "Beverage",
            1200,
            ProductStatus.Active,
            25,
            5);

        var actionResult = await controller.CreateAsync(request, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(ProductsController.GetAsync), createdResult.ActionName);
        Assert.Equal(productId, createdResult.RouteValues!["id"]);
        Assert.Same(response, createdResult.Value);
        Assert.NotNull(service.CapturedCreateCommand);
        Assert.Equal(request.Sku, service.CapturedCreateCommand.Sku);
        Assert.Equal(request.Name, service.CapturedCreateCommand.Name);
        Assert.Equal(request.Description, service.CapturedCreateCommand.Description);
        Assert.Equal(request.Category, service.CapturedCreateCommand.Category);
        Assert.Equal(request.Price, service.CapturedCreateCommand.Price);
        Assert.Equal(request.Status, service.CapturedCreateCommand.Status);
        Assert.Equal(request.InitialQuantity, service.CapturedCreateCommand.InitialQuantity);
        Assert.Equal(request.SafetyStock, service.CapturedCreateCommand.SafetyStock);
    }

    [Fact]
    public async Task GetAsync_WhenServiceFailsReturnsNotImplementedWithMessage()
    {
        const string error = "Not implemented.";
        var service = new CapturingProductService
        {
            GetResult = ProductResult<ProductDetailResponse>.Failure(error)
        };
        var controller = new ProductsController(service);

        var actionResult = await controller.GetAsync(Guid.NewGuid(), CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status501NotImplemented, objectResult.StatusCode);
        Assert.Equal(error, objectResult.Value?.GetType().GetProperty("message")?.GetValue(objectResult.Value));
    }

    [Fact]
    public async Task DeleteAsync_WhenServiceSucceedsReturnsNoContent()
    {
        var service = new CapturingProductService
        {
            DeleteResult = ProductResult<bool>.Success(true)
        };
        var controller = new ProductsController(service);

        var actionResult = await controller.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NoContentResult>(actionResult);
    }

    private sealed class CapturingProductService : IProductService
    {
        public ListProductsQuery? CapturedListQuery { get; private set; }

        public CreateProductCommand? CapturedCreateCommand { get; private set; }

        public ProductResult<ProductListResponse> ListResult { get; init; } =
            ProductResult<ProductListResponse>.Failure("Unexpected call.");

        public ProductResult<ProductDetailResponse> GetResult { get; init; } =
            ProductResult<ProductDetailResponse>.Failure("Unexpected call.");

        public ProductResult<ProductDetailResponse> CreateResult { get; init; } =
            ProductResult<ProductDetailResponse>.Failure("Unexpected call.");

        public ProductResult<ProductDetailResponse> UpdateResult { get; init; } =
            ProductResult<ProductDetailResponse>.Failure("Unexpected call.");

        public ProductResult<bool> DeleteResult { get; init; } = ProductResult<bool>.Failure("Unexpected call.");

        public Task<ProductResult<ProductListResponse>> ListAsync(
            ListProductsQuery query,
            CancellationToken cancellationToken = default)
        {
            CapturedListQuery = query;
            return Task.FromResult(ListResult);
        }

        public Task<ProductResult<ProductDetailResponse>> GetAsync(
            GetProductQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetResult);
        }

        public Task<ProductResult<ProductDetailResponse>> CreateAsync(
            CreateProductCommand command,
            CancellationToken cancellationToken = default)
        {
            CapturedCreateCommand = command;
            return Task.FromResult(CreateResult);
        }

        public Task<ProductResult<ProductDetailResponse>> UpdateAsync(
            UpdateProductCommand command,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UpdateResult);
        }

        public Task<ProductResult<bool>> DeleteAsync(
            DeleteProductCommand command,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DeleteResult);
        }
    }
}
