using AspNetNextApp.Api.Attribute;
using AspNetNextApp.Api.Contracts.Products;
using AspNetNextApp.Api.Enums;
using AspNetNextApp.Api.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Controllers
{
    [ApiController]
    [Route("products")]
    [Authorize]
    public sealed class ProductsController(IProductService productService) : ControllerBase
    {
        [HttpGet]
        [UserRole(UserRole.Admin, UserRole.Staff, UserRole.Viewer)]
        [ProducesResponseType(typeof(ProductListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProductListResponse>> ListAsync(
            [FromQuery] ListProductsRequest request,
            CancellationToken cancellationToken = default)
        {
            ProductResult<ProductListResponse> result = await productService.ListAsync(
                new ListProductsQuery(
                    request.Query,
                    request.Status,
                    request.Category,
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
        [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDetailResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            GetProductRequest request = new(id);
            ProductResult<ProductDetailResponse> result = await productService.GetAsync(new GetProductQuery(request.Id), cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost]
        [UserRole(UserRole.Admin)]
        [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductDetailResponse>> CreateAsync(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            ProductResult<ProductDetailResponse> result = await productService.CreateAsync(
                new CreateProductCommand(
                    request.Sku,
                    request.Name,
                    request.Description,
                    request.Category,
                    request.Price,
                    request.Status,
                    request.InitialQuantity,
                    request.SafetyStock),
                cancellationToken);

            return !result.IsSuccess ? ToActionResult(result) : (ActionResult<ProductDetailResponse>)CreatedAtAction(nameof(GetAsync), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id:guid}")]
        [UserRole(UserRole.Admin)]
        [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductDetailResponse>> UpdateAsync(
            Guid id,
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            ProductResult<ProductDetailResponse> result = await productService.UpdateAsync(
                new UpdateProductCommand(
                    id,
                    request.Sku,
                    request.Name,
                    request.Description,
                    request.Category,
                    request.Price,
                    request.Status),
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [UserRole(UserRole.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            DeleteProductRequest request = new(id);
            ProductResult<bool> result = await productService.DeleteAsync(new DeleteProductCommand(request.Id), cancellationToken);

            return !result.IsSuccess ? ToErrorActionResult(result) : NoContent();
        }

        private ActionResult<T> ToActionResult<T>(ProductResult<T> result)
        {
            return result.IsSuccess ? (ActionResult<T>)Ok(result.Value) : (ActionResult<T>)ToErrorActionResult(result);
        }


        private ObjectResult ToErrorActionResult<T>(ProductResult<T> result)
        {
            int statusCode = result.ErrorType switch
            {
                ProductErrorType.NotFound => StatusCodes.Status404NotFound,
                ProductErrorType.Conflict => StatusCodes.Status409Conflict,
                ProductErrorType.Validation => StatusCodes.Status400BadRequest,
                null => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest,
            };

            return StatusCode(statusCode, new { message = result.Error });
        }
    }
}
