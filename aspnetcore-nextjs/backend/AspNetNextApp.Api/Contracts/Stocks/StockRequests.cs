using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Contracts.Stocks
{
    public sealed class ListStocksRequest
    {
        [FromQuery(Name = "product_id")]
        public Guid? ProductId { get; init; }

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

    public sealed record GetStockRequest(Guid Id);

    public sealed record CreateStockRequest(
        Guid ProductId,

        [property: Range(0, int.MaxValue)]
        int Quantity,

        [property: Range(0, int.MaxValue)]
        int SafetyStock);

    public sealed record UpdateStockRequest(
        [property: Range(0, int.MaxValue)]
        int Quantity,

        [property: Range(0, int.MaxValue)]
        int SafetyStock,

        [property: MaxLength(255)]
        string? Reason);
}
