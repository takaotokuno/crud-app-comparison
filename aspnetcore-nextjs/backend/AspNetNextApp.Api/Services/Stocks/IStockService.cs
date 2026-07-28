using AspNetNextApp.Api.Contracts.Stocks;

namespace AspNetNextApp.Api.Services.Stocks
{
    public interface IStockService
    {
        Task<StockResult<StockListResponse>> ListAsync(ListStocksQuery query, CancellationToken cancellationToken = default);

        Task<StockResult<StockDetailResponse>> GetAsync(GetStockQuery query, CancellationToken cancellationToken = default);

        Task<StockResult<StockDetailResponse>> CreateAsync(CreateStockCommand command, CancellationToken cancellationToken = default);

        Task<StockResult<StockDetailResponse>> UpdateAsync(UpdateStockCommand command, CancellationToken cancellationToken = default);

        Task<StockResult<StockDetailResponse>> AdjustAsync(AdjustStockCommand command, CancellationToken cancellationToken = default);
    }
}
