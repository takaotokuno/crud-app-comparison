using AspNetNextApp.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Contracts.Products;

public sealed class ListProductsRequest
{
    [FromQuery(Name = "q")]
    public string? Query { get; init; }

    [FromQuery]
    public ProductStatus? Status { get; init; }

    [FromQuery]
    public string? Category { get; init; }

    [FromQuery(Name = "low_stock")]
    public bool? LowStock { get; init; }

    [FromQuery(Name = "sort_by")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sort_direction")]
    public string? SortDirection { get; init; }

    [FromQuery]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "page_size")]
    public int PageSize { get; init; } = 20;
}

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
