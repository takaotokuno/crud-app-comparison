using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Contracts.Products;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class ProductsControllerTests
    {
        [Fact]
        public async Task ListAsync_ForwardsQueryParametersAndReturnsOk()
        {
            ProductListResponse response = new ProductListResponse([], 2, 10, 0);
            CapturingProductService service = new CapturingProductService
            {
                ListResult = ProductResult<ProductListResponse>.Success(response)
            };
            ProductsController controller = new ProductsController(service);

            ListProductsRequest request = new ListProductsRequest
            {
                Query = "coffee",
                Status = ProductStatus.Active,
                Category = "Beverage",
                LowStock = true,
                SortBy = "name",
                SortDirection = "desc",
                Page = 2,
                PageSize = 10,
            };

            ActionResult<ProductListResponse> actionResult = await controller.ListAsync(request, CancellationToken.None);

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
            Guid productId = Guid.NewGuid();
            DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddMinutes(-5);
            DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
            ProductDetailResponse response = new ProductDetailResponse(
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
            CapturingProductService service = new CapturingProductService
            {
                CreateResult = ProductResult<ProductDetailResponse>.Success(response)
            };
            ProductsController controller = new ProductsController(service);
            CreateProductRequest request = new CreateProductRequest(
                "SKU-001",
                "Coffee Beans",
                "Medium roast",
                "Beverage",
                1200,
                ProductStatus.Active,
                25,
                5);

            ActionResult<ProductDetailResponse> actionResult = await controller.CreateAsync(request, CancellationToken.None);

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
        public async Task GetAsync_WhenServiceFailsReturnsBadRequestWithMessage()
        {
            const string error = "Not implemented.";
            CapturingProductService service = new CapturingProductService
            {
                GetResult = ProductResult<ProductDetailResponse>.Failure(error)
            };
            ProductsController controller = new ProductsController(service);

            ActionResult<ProductDetailResponse> actionResult = await controller.GetAsync(Guid.NewGuid(), CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.Equal(error, objectResult.Value?.GetType().GetProperty("message")?.GetValue(objectResult.Value));
        }

        [Fact]
        public async Task DeleteAsync_WhenServiceSucceedsReturnsNoContent()
        {
            CapturingProductService service = new CapturingProductService
            {
                DeleteResult = ProductResult<bool>.Success(true)
            };
            ProductsController controller = new ProductsController(service);

            IActionResult actionResult = await controller.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public void Controller_RequiresAuthenticatedUsers()
        {
            var attribute = Assert.Single(
                typeof(ProductsController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));

            Assert.IsType<AuthorizeAttribute>(attribute);
        }

        [Theory]
        [InlineData(nameof(ProductsController.ListAsync), "Admin,Staff,Viewer")]
        [InlineData(nameof(ProductsController.GetAsync), "Admin,Staff,Viewer")]
        [InlineData(nameof(ProductsController.CreateAsync), "Admin")]
        [InlineData(nameof(ProductsController.UpdateAsync), "Admin")]
        [InlineData(nameof(ProductsController.DeleteAsync), "Admin")]
        public void ProductEndpoints_DeclareExpectedRoles(string actionName, string expectedRoles)
        {
            System.Reflection.MethodInfo method = typeof(ProductsController).GetMethods()
                .Single(method => method.Name == actionName);

            var attribute = Assert.Single(
                method.GetCustomAttributes(typeof(UserRoleAttribute), inherit: true));
            var roleAttribute = Assert.IsType<UserRoleAttribute>(attribute);

            Assert.Equal(expectedRoles, roleAttribute.Roles);
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
}
