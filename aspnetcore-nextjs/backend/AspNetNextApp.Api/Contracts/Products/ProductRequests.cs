using System.ComponentModel.DataAnnotations;

using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Contracts.Products
{
    public sealed class ListProductsRequest
    {
        [FromQuery(Name = "q")]
        [MaxLength(100)]
        public string? Query { get; init; }

        [FromQuery]
        public ProductStatus? Status { get; init; }

        [FromQuery]
        [MaxLength(50)]
        public string? Category { get; init; }

        [FromQuery(Name = "low_stock")]
        public bool? LowStock { get; init; }

        [FromQuery(Name = "sort_by")]
        [MaxLength(32)]
        public string? SortBy { get; init; }

        [FromQuery(Name = "sort_direction")]
        [MaxLength(4)]
        public string? SortDirection { get; init; }

        [FromQuery]
        [Range(1, int.MaxValue)]
        public int Page { get; init; } = 1;

        [FromQuery(Name = "page_size")]
        [Range(1, 100)]
        public int PageSize { get; init; } = 20;
    }

    public sealed record GetProductRequest(Guid Id);

    public sealed record CreateProductRequest(
        [param: Required]
        [param: RegularExpression(@"^[A-Za-z0-9_-]{1,32}$")]
        [param: MaxLength(32)]
        string Sku,

        [param: Required]
        [param: MaxLength(100)]
        string Name,

        [param: MaxLength(1000)]
        string? Description,

        [param: MaxLength(50)]
        string? Category,

        [param: Range(0, int.MaxValue)]
        int Price,

        ProductStatus Status,

        [param: Range(0, int.MaxValue)]
        int InitialQuantity,

        [param: Range(0, int.MaxValue)]
        int SafetyStock);

    public sealed record UpdateProductRequest(
        Guid Id,

        [param: Required]
        [param: RegularExpression(@"^[A-Za-z0-9_-]{1,32}$")]
        [param: MaxLength(32)]
        string Sku,

        [param: Required]
        [param: MaxLength(100)]
        string Name,

        [param: MaxLength(1000)]
        string? Description,

        [param: MaxLength(50)]
        string? Category,

        [param: Range(0, int.MaxValue)]
        int Price,

        ProductStatus Status);

    public sealed record DeleteProductRequest(Guid Id);
}
