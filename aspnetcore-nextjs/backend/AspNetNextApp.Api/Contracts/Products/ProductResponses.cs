using AspNetNextApp.Api.Entities;

namespace AspNetNextApp.Api.Contracts.Products;

public sealed record ProductListResponse(IReadOnlyCollection<ProductSummaryResponse> Items, int Page, int PageSize, int TotalCount);

public sealed record ProductSummaryResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Category,
    int Price,
    ProductStatus Status,
    int Quantity,
    int SafetyStock,
    DateTimeOffset UpdatedAt);

public sealed record ProductDetailResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string? Category,
    int Price,
    ProductStatus Status,
    int Quantity,
    int SafetyStock,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
