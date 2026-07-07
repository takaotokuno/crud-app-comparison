using AspNetNextApp.Api.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AspNetNextApp.Api.Contracts.StockTransactions
{
    public sealed class ListStockTransactionsRequest
    {
        [FromQuery(Name = "product_id")]
        public Guid? ProductId { get; init; }

        [FromQuery]
        public StockTransactionType? Type { get; init; }

        [FromQuery]
        public int Page { get; init; } = 1;

        [FromQuery(Name = "page_size")]
        public int PageSize { get; init; } = 20;
    }

    public sealed record CreateStockTransactionRequest(
        Guid ProductId,
        StockTransactionType Type,
        int QuantityDelta,
        string? Reason);
}
