namespace AspNetNextApp.Api.Services.Stocks
{
    public sealed record ListStocksQuery(Guid? ProductId, bool? LowStock, string? SortBy, string? SortDirection, int Page, int PageSize);

    public sealed record GetStockQuery(Guid Id);

    public sealed record CreateStockCommand(Guid ProductId, int Quantity, int SafetyStock);

    public sealed record UpdateStockCommand(Guid Id, int Quantity, int SafetyStock, string? Reason);
}
