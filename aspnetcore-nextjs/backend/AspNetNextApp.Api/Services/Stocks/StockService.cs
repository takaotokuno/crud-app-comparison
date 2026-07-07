using AspNetNextApp.Api.Contracts.Stocks;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.Stocks
{
    public sealed class StockService(AppDbContext dbContext) : IStockService
    {
        private const int MaxPageSize = 100;

        public async Task<StockResult<StockListResponse>> ListAsync(ListStocksQuery query, CancellationToken cancellationToken = default)
        {
            int page = Math.Max(query.Page, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            IQueryable<Stock> stocks = dbContext.Stocks
                .AsNoTracking()
                .Include(stock => stock.Product)
                .AsQueryable();

            if (query.ProductId.HasValue)
            {
                stocks = stocks.Where(stock => stock.ProductId == query.ProductId.Value);
            }

            if (query.LowStock == true)
            {
                stocks = stocks.Where(stock => stock.Quantity <= stock.SafetyStock);
            }

            int totalCount = await stocks.CountAsync(cancellationToken);
            List<StockSummaryResponse> items = await ApplySort(stocks, query.SortBy, query.SortDirection)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(stock => new StockSummaryResponse(
                    stock.Id,
                    stock.ProductId,
                    stock.Product.Sku,
                    stock.Product.Name,
                    stock.Quantity,
                    stock.SafetyStock,
                    stock.Quantity <= stock.SafetyStock,
                    stock.UpdatedAt))
                .ToListAsync(cancellationToken);

            return StockResult<StockListResponse>.Success(new StockListResponse(items, page, pageSize, totalCount));
        }

        public async Task<StockResult<StockDetailResponse>> GetAsync(GetStockQuery query, CancellationToken cancellationToken = default)
        {
            Stock? stock = await FindStockAsync(query.Id, cancellationToken);

            return stock is null
                ? StockResult<StockDetailResponse>.Failure("Stock was not found.", StockErrorType.NotFound)
                : StockResult<StockDetailResponse>.Success(ToDetailResponse(stock));
        }


        public async Task<StockResult<StockDetailResponse>> CreateAsync(CreateStockCommand command, CancellationToken cancellationToken = default)
        {
            string? validationError = ValidateStockInput(command.Quantity, command.SafetyStock, reason: null);
            if (validationError is not null)
            {
                return StockResult<StockDetailResponse>.Failure(validationError);
            }

            Product? product = await dbContext.Products
                .Include(product => product.Stock)
                .FirstOrDefaultAsync(product => product.Id == command.ProductId, cancellationToken);
            if (product is null)
            {
                return StockResult<StockDetailResponse>.Failure("Product was not found.", StockErrorType.NotFound);
            }

            if (product.Stock is not null)
            {
                return StockResult<StockDetailResponse>.Failure("Stock already exists for the product.", StockErrorType.Conflict);
            }

            Stock stock = Stock.Create(product, command.Quantity, command.SafetyStock);
            _ = dbContext.Stocks.Add(stock);
            _ = await dbContext.SaveChangesAsync(cancellationToken);

            return StockResult<StockDetailResponse>.Success(ToDetailResponse(stock));
        }

        public async Task<StockResult<StockDetailResponse>> UpdateAsync(UpdateStockCommand command, CancellationToken cancellationToken = default)
        {
            string? validationError = ValidateStockInput(command.Quantity, command.SafetyStock, command.Reason);
            if (validationError is not null)
            {
                return StockResult<StockDetailResponse>.Failure(validationError);
            }

            Stock? stock = await FindStockAsync(command.Id, cancellationToken);
            if (stock is null)
            {
                return StockResult<StockDetailResponse>.Failure("Stock was not found.", StockErrorType.NotFound);
            }

            if (stock.Quantity != command.Quantity)
            {
                StockTransaction transaction = stock.AdjustTo(command.Quantity, command.Reason);
                _ = dbContext.StockTransactions.Add(transaction);
            }

            stock.UpdateSafetyStock(command.SafetyStock);
            _ = await dbContext.SaveChangesAsync(cancellationToken);

            return StockResult<StockDetailResponse>.Success(ToDetailResponse(stock));
        }

        private static IQueryable<Stock> ApplySort(IQueryable<Stock> stocks, string? sortBy, string? sortDirection)
        {
            bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "quantity" => descending ? stocks.OrderByDescending(stock => stock.Quantity) : stocks.OrderBy(stock => stock.Quantity),
                "safety_stock" => descending ? stocks.OrderByDescending(stock => stock.SafetyStock) : stocks.OrderBy(stock => stock.SafetyStock),
                "product_sku" => descending ? stocks.OrderByDescending(stock => stock.Product.Sku) : stocks.OrderBy(stock => stock.Product.Sku),
                "created_at" => descending ? stocks.OrderByDescending(stock => stock.CreatedAt) : stocks.OrderBy(stock => stock.CreatedAt),
                "updated_at" or _ => descending || string.IsNullOrWhiteSpace(sortDirection)
                    ? stocks.OrderByDescending(stock => stock.UpdatedAt)
                    : stocks.OrderBy(stock => stock.UpdatedAt),
            };
        }

        private static string? ValidateStockInput(int quantity, int safetyStock, string? reason)
        {
            if (quantity < 0)
            {
                return "Quantity must be zero or greater.";
            }

            if (safetyStock < 0)
            {
                return "Safety stock must be zero or greater.";
            }

            return reason?.Length > 255 ? "Reason must be 255 characters or fewer." : null;
        }

        private Task<Stock?> FindStockAsync(Guid id, CancellationToken cancellationToken)
        {
            return dbContext.Stocks
                .Include(stock => stock.Product)
                .FirstOrDefaultAsync(stock => stock.Id == id, cancellationToken);
        }

        private static StockDetailResponse ToDetailResponse(Stock stock)
        {
            return new(
                stock.Id,
                stock.ProductId,
                stock.Product.Sku,
                stock.Product.Name,
                stock.Quantity,
                stock.SafetyStock,
                stock.Quantity <= stock.SafetyStock,
                stock.CreatedAt,
                stock.UpdatedAt);
        }
    }
}
