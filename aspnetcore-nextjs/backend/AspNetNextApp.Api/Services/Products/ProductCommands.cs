using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Services.Products
{
    public sealed record ListProductsQuery(
        string? Query,
        ProductStatus? Status,
        string? Category,
        bool? LowStock,
        string? SortBy,
        string? SortDirection,
        int Page,
        int PageSize);

    public sealed record GetProductQuery(Guid Id);

    public sealed record CreateProductCommand(
        string Sku,
        string Name,
        string? Description,
        string? Category,
        int Price,
        ProductStatus Status,
        int InitialQuantity,
        int SafetyStock);

    public sealed record UpdateProductCommand(
        Guid Id,
        string Sku,
        string Name,
        string? Description,
        string? Category,
        int Price,
        ProductStatus Status);

    public sealed record DeleteProductCommand(Guid Id);
}
