using AspNetNextApp.Api.Entities;

namespace AspNetNextApp.Api.Contracts.Products;

public sealed record ListProductsRequest(
    string? Query,
    ProductStatus? Status,
    string? Category,
    bool? LowStock,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int PageSize = 20);

public sealed record GetProductRequest(Guid Id);

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    string? Category,
    int Price,
    ProductStatus Status,
    int InitialQuantity,
    int SafetyStock);

public sealed record UpdateProductRequest(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? Category,
    int Price,
    ProductStatus Status);

public sealed record DeleteProductRequest(Guid Id);
