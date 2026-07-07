using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Contracts.Stocks;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Stocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("api/stocks")]
    [Authorize]
    public sealed class StocksController(IStockService stockService) : ControllerBase
    {
        [HttpGet]
        [UserRole(UserRole.Admin, UserRole.Staff, UserRole.Viewer)]
        [ProducesResponseType(typeof(StockListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StockListResponse>> ListAsync(
            [FromQuery] ListStocksRequest request,
            CancellationToken cancellationToken = default)
        {
            StockResult<StockListResponse> result = await stockService.ListAsync(
                new ListStocksQuery(
                    request.ProductId,
                    request.LowStock,
                    request.SortBy,
                    request.SortDirection,
                    request.Page,
                    request.PageSize),
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [UserRole(UserRole.Admin, UserRole.Staff, UserRole.Viewer)]
        [ProducesResponseType(typeof(StockDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockDetailResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            GetStockRequest request = new(id);
            StockResult<StockDetailResponse> result = await stockService.GetAsync(new GetStockQuery(request.Id), cancellationToken);

            return ToActionResult(result);
        }


        [HttpPost]
        [UserRole(UserRole.Admin)]
        [ProducesResponseType(typeof(StockDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<StockDetailResponse>> CreateAsync(
            [FromBody] CreateStockRequest request,
            CancellationToken cancellationToken)
        {
            StockResult<StockDetailResponse> result = await stockService.CreateAsync(
                new CreateStockCommand(request.ProductId, request.Quantity, request.SafetyStock),
                cancellationToken);

            return !result.IsSuccess ? ToActionResult(result) : (ActionResult<StockDetailResponse>)CreatedAtAction(nameof(GetAsync), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:guid}")]
        [UserRole(UserRole.Admin, UserRole.Staff)]
        [ProducesResponseType(typeof(StockDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockDetailResponse>> UpdateAsync(
            Guid id,
            [FromBody] UpdateStockRequest request,
            CancellationToken cancellationToken)
        {
            StockResult<StockDetailResponse> result = await stockService.UpdateAsync(
                new UpdateStockCommand(id, request.Quantity, request.SafetyStock, request.Reason),
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPatch("{id:guid}")]
        [UserRole(UserRole.Admin, UserRole.Staff)]
        [ProducesResponseType(typeof(StockDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult<StockDetailResponse>> PatchAsync(
            Guid id,
            [FromBody] UpdateStockRequest request,
            CancellationToken cancellationToken)
        {
            return UpdateAsync(id, request, cancellationToken);
        }

        private ActionResult<T> ToActionResult<T>(StockResult<T> result)
        {
            return result.IsSuccess ? (ActionResult<T>)Ok(result.Value) : (ActionResult<T>)ToErrorActionResult(result);
        }

        private ObjectResult ToErrorActionResult<T>(StockResult<T> result)
        {
            int statusCode = result.ErrorType switch
            {
                StockErrorType.NotFound => StatusCodes.Status404NotFound,
                StockErrorType.Conflict => StatusCodes.Status409Conflict,
                StockErrorType.Validation => StatusCodes.Status400BadRequest,
                null => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest,
            };

            return StatusCode(statusCode, new { message = result.Error });
        }
    }
}
