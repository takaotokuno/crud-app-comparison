using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Contracts.Stocks;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Services.Stocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class StocksControllerTests
    {
        [Fact]
        public async Task ListAsync_ForwardsQueryParametersAndReturnsOk()
        {
            StockListResponse response = new([], 2, 10, 0);
            CapturingStockService service = new() { ListResult = StockResult<StockListResponse>.Success(response) };
            StocksController controller = new(service);
            Guid productId = Guid.NewGuid();
            ListStocksRequest request = new()
            {
                ProductId = productId,
                LowStock = true,
                SortBy = "quantity",
                SortDirection = "desc",
                Page = 2,
                PageSize = 10,
            };

            ActionResult<StockListResponse> actionResult = await controller.ListAsync(request, CancellationToken.None);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Same(response, okResult.Value);
            Assert.NotNull(service.CapturedListQuery);
            Assert.Equal(productId, service.CapturedListQuery.ProductId);
            Assert.True(service.CapturedListQuery.LowStock);
            Assert.Equal("quantity", service.CapturedListQuery.SortBy);
            Assert.Equal("desc", service.CapturedListQuery.SortDirection);
            Assert.Equal(2, service.CapturedListQuery.Page);
            Assert.Equal(10, service.CapturedListQuery.PageSize);
        }

        [Fact]
        public async Task GetAsync_WhenServiceReturnsNotFoundReturnsNotFoundWithMessage()
        {
            const string error = "Stock was not found.";
            CapturingStockService service = new()
            {
                GetResult = StockResult<StockDetailResponse>.Failure(error, StockErrorType.NotFound)
            };
            StocksController controller = new(service);

            ActionResult<StockDetailResponse> actionResult = await controller.GetAsync(Guid.NewGuid(), CancellationToken.None);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal(error, objectResult.Value?.GetType().GetProperty("message")?.GetValue(objectResult.Value));
        }

        [Fact]
        public async Task UpdateAsync_ForwardsBodyAndReturnsOk()
        {
            Guid stockId = Guid.NewGuid();
            StockDetailResponse response = new(stockId, Guid.NewGuid(), "SKU-001", "Coffee Beans", 8, 3, false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
            CapturingStockService service = new() { UpdateResult = StockResult<StockDetailResponse>.Success(response) };
            StocksController controller = new(service);
            UpdateStockRequest request = new(3);

            ActionResult<StockDetailResponse> actionResult = await controller.UpdateAsync(stockId, request, CancellationToken.None);

            OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Same(response, okResult.Value);
            Assert.NotNull(service.CapturedUpdateCommand);
            Assert.Equal(stockId, service.CapturedUpdateCommand.Id);
            Assert.Equal(3, service.CapturedUpdateCommand.SafetyStock);
        }

        [Fact]
        public void Controller_RequiresAuthenticatedUsers()
        {
            object attribute = Assert.Single(typeof(StocksController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
            _ = Assert.IsType<AuthorizeAttribute>(attribute);
        }

        [Theory]
        [InlineData(nameof(StocksController.ListAsync), "Admin,Staff,Viewer")]
        [InlineData(nameof(StocksController.GetAsync), "Admin,Staff,Viewer")]
        [InlineData(nameof(StocksController.CreateAsync), "Admin")]
        [InlineData(nameof(StocksController.UpdateAsync), "Admin,Staff")]
        [InlineData(nameof(StocksController.PatchAsync), "Admin,Staff")]
        [InlineData(nameof(StocksController.AdjustAsync), "Admin")]
        public void StockEndpoints_DeclareExpectedRoles(string actionName, string expectedRoles)
        {
            System.Reflection.MethodInfo method = typeof(StocksController).GetMethods().Single(method => method.Name == actionName);
            object attribute = Assert.Single(method.GetCustomAttributes(typeof(UserRoleAttribute), inherit: true));
            UserRoleAttribute roleAttribute = Assert.IsType<UserRoleAttribute>(attribute);

            Assert.Equal(expectedRoles, roleAttribute.Roles);
        }

        private sealed class CapturingStockService : IStockService
        {
            public ListStocksQuery? CapturedListQuery { get; private set; }
            public UpdateStockCommand? CapturedUpdateCommand { get; private set; }
            public AdjustStockCommand? CapturedAdjustCommand { get; private set; }
            public StockResult<StockListResponse> ListResult { get; init; } = StockResult<StockListResponse>.Failure("Unexpected call.");
            public StockResult<StockDetailResponse> GetResult { get; init; } = StockResult<StockDetailResponse>.Failure("Unexpected call.");
            public StockResult<StockDetailResponse> CreateResult { get; init; } = StockResult<StockDetailResponse>.Failure("Unexpected call.");
            public StockResult<StockDetailResponse> UpdateResult { get; init; } = StockResult<StockDetailResponse>.Failure("Unexpected call.");
            public StockResult<StockDetailResponse> AdjustResult { get; init; } = StockResult<StockDetailResponse>.Failure("Unexpected call.");

            public Task<StockResult<StockListResponse>> ListAsync(ListStocksQuery query, CancellationToken cancellationToken = default)
            {
                CapturedListQuery = query;
                return Task.FromResult(ListResult);
            }

            public Task<StockResult<StockDetailResponse>> GetAsync(GetStockQuery query, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(GetResult);
            }

            public Task<StockResult<StockDetailResponse>> CreateAsync(CreateStockCommand command, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(CreateResult);
            }

            public Task<StockResult<StockDetailResponse>> UpdateAsync(UpdateStockCommand command, CancellationToken cancellationToken = default)
            {
                CapturedUpdateCommand = command;
                return Task.FromResult(UpdateResult);
            }

            public Task<StockResult<StockDetailResponse>> AdjustAsync(AdjustStockCommand command, CancellationToken cancellationToken = default)
            {
                CapturedAdjustCommand = command;
                return Task.FromResult(AdjustResult);
            }
        }
    }
}
