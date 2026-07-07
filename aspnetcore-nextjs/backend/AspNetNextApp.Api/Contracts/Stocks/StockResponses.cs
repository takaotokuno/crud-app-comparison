namespace AspNetNextApp.Api.Contracts.Stocks
{
    public sealed record StockListResponse(IReadOnlyCollection<StockSummaryResponse> Items, int Page, int PageSize, int TotalCount);

    public sealed record StockSummaryResponse(
        Guid Id,
        Guid ProductId,
        string ProductSku,
        string ProductName,
        int Quantity,
        int SafetyStock,
        bool IsLowStock,
        DateTimeOffset UpdatedAt);

    public sealed record StockDetailResponse(
        Guid Id,
        Guid ProductId,
        string ProductSku,
        string ProductName,
        int Quantity,
        int SafetyStock,
        bool IsLowStock,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);
}
