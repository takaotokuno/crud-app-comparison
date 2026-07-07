using AspNetNextApp.Api.Enums;

namespace AspNetNextApp.Api.Contracts.StockTransactions
{
    public sealed record StockTransactionListResponse(
        IReadOnlyCollection<StockTransactionResponse> Items,
        int Page,
        int PageSize,
        int TotalCount);

    public sealed record StockTransactionResponse(
        Guid Id,
        Guid ProductId,
        Guid StockId,
        StockTransactionType Type,
        int QuantityDelta,
        int QuantityAfter,
        string? Reason,
        Guid? CreatedById,
        DateTimeOffset CreatedAt);
}
