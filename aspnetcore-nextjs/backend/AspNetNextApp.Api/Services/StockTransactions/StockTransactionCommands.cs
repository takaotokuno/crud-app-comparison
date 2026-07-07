using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Services.StockTransactions
{
    public sealed record ListStockTransactionsQuery(
        Guid? ProductId,
        StockTransactionType? Type,
        int Page,
        int PageSize);

    public sealed record CreateStockTransactionCommand(
        Guid ProductId,
        StockTransactionType Type,
        int QuantityDelta,
        string? Reason,
        Guid? CreatedById);
}
