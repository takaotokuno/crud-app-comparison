using System.Security.Claims;

using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Contracts.StockTransactions;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.StockTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("api/stock-transactions")]
    [Authorize]
    public sealed class StockTransactionsController(IStockTransactionService stockTransactionService) : ControllerBase
    {
        [HttpGet]
        [UserRole(UserRole.Admin, UserRole.Staff, UserRole.Viewer)]
        [ProducesResponseType(typeof(StockTransactionListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StockTransactionListResponse>> ListAsync(
            [FromQuery] ListStockTransactionsRequest request,
            CancellationToken cancellationToken = default)
        {
            StockTransactionResult<StockTransactionListResponse> result = await stockTransactionService.ListAsync(
                new ListStockTransactionsQuery(request.ProductId, request.Type, request.Page, request.PageSize),
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost]
        [UserRole(UserRole.Admin, UserRole.Staff)]
        [ProducesResponseType(typeof(StockTransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockTransactionResponse>> CreateAsync(
            [FromBody] CreateStockTransactionRequest request,
            CancellationToken cancellationToken = default)
        {
            StockTransactionResult<StockTransactionResponse> result = await stockTransactionService.CreateAsync(
                new CreateStockTransactionCommand(
                    request.ProductId,
                    request.Type,
                    request.QuantityDelta,
                    request.Reason,
                    GetCurrentUserId()),
                cancellationToken);

            return !result.IsSuccess
                ? ToActionResult(result)
                : (ActionResult<StockTransactionResponse>)CreatedAtAction(nameof(ListAsync), new { product_id = result.Value!.ProductId }, result.Value);
        }

        private Guid? GetCurrentUserId()
        {
            string? idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idValue, out Guid id) ? id : null;
        }

        private ActionResult<T> ToActionResult<T>(StockTransactionResult<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            int statusCode = result.ErrorType switch
            {
                StockTransactionErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest,
            };

            return StatusCode(statusCode, new { message = result.Error });
        }
    }
}
