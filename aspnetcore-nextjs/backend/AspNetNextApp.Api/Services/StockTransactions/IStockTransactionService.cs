using AspNetNextApp.Api.Contracts.StockTransactions;

namespace AspNetNextApp.Api.Services.StockTransactions
{
    public interface IStockTransactionService
    {
        Task<StockTransactionResult<StockTransactionListResponse>> ListAsync(ListStockTransactionsQuery query, CancellationToken cancellationToken = default);

        Task<StockTransactionResult<StockTransactionResponse>> CreateAsync(CreateStockTransactionCommand command, CancellationToken cancellationToken = default);
    }
}
