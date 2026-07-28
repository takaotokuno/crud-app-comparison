using AspNetNextApp.Api.Contracts.StockTransactions;
using AspNetNextApp.Api.Controllers;
using AspNetNextApp.Api.Services.StockTransactions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace AspNetNextApp.Api.Tests.Controllers
{
    public sealed class StockTransactionsControllerTests
    {
        [Theory]
        [InlineData(StockTransactionErrorType.Validation)]
        [InlineData(null)]
        public async Task ListAsync_WhenServiceReturnsInvalidFailureReturnsBadRequest(StockTransactionErrorType? errorType)
        {
            StubStockTransactionService service = new(
                new StockTransactionResult<StockTransactionListResponse>(null, false, "Invalid transaction query.", errorType));
            StockTransactionsController controller = new(service);

            ActionResult<StockTransactionListResponse> actionResult = await controller.ListAsync(
                new ListStockTransactionsRequest(),
                CancellationToken.None);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        }

        private sealed class StubStockTransactionService(
            StockTransactionResult<StockTransactionListResponse> listResult) : IStockTransactionService
        {
            public Task<StockTransactionResult<StockTransactionListResponse>> ListAsync(
                ListStockTransactionsQuery query,
                CancellationToken cancellationToken = default) => Task.FromResult(listResult);

            public Task<StockTransactionResult<StockTransactionResponse>> CreateAsync(
                CreateStockTransactionCommand command,
                CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }
    }
}
