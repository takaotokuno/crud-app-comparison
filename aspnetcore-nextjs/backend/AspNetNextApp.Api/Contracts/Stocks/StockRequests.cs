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
        public string? SortBy { get; init; }

        [FromQuery(Name = "sort_direction")]
        public string? SortDirection { get; init; }

        [FromQuery]
        public int Page { get; init; } = 1;

        [FromQuery(Name = "page_size")]
        public int PageSize { get; init; } = 20;
    }

    public sealed record GetStockRequest(Guid Id);

    public sealed record CreateStockRequest(Guid ProductId, int Quantity, int SafetyStock);

    public sealed record UpdateStockRequest(int Quantity, int SafetyStock, string? Reason);
}
