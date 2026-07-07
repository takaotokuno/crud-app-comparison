using AspNetNextApp.Api.Contracts.StockTransactions;
using AspNetNextApp.Api.Data;
using AspNetNextApp.Api.Entities;
using AspNetNextApp.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace AspNetNextApp.Api.Services.StockTransactions
{
    public sealed class StockTransactionService(AppDbContext dbContext) : IStockTransactionService
    {
        private const int MaxPageSize = 100;

        public async Task<StockTransactionResult<StockTransactionListResponse>> ListAsync(
            ListStockTransactionsQuery query,
            CancellationToken cancellationToken = default)
        {
            int page = Math.Max(query.Page, 1);
            int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            IQueryable<StockTransaction> transactions = dbContext.StockTransactions
                .AsNoTracking()
                .OrderByDescending(transaction => transaction.CreatedAt)
                .AsQueryable();

            if (query.ProductId.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.ProductId == query.ProductId.Value);
            }

            if (query.Type.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Type == query.Type.Value);
            }

            int totalCount = await transactions.CountAsync(cancellationToken);
            List<StockTransactionResponse> items = await transactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(transaction => ToResponse(transaction))
                .ToListAsync(cancellationToken);

            return StockTransactionResult<StockTransactionListResponse>.Success(new StockTransactionListResponse(items, page, pageSize, totalCount));
        }

        public async Task<StockTransactionResult<StockTransactionResponse>> CreateAsync(
            CreateStockTransactionCommand command,
            CancellationToken cancellationToken = default)
        {
            string? validationError = ValidateCommand(command);
            if (validationError is not null)
            {
                return StockTransactionResult<StockTransactionResponse>.Failure(validationError);
            }

            Stock? stock = await dbContext.Stocks
                .Include(candidate => candidate.Product)
                .FirstOrDefaultAsync(candidate => candidate.ProductId == command.ProductId, cancellationToken);
            if (stock is null)
            {
                return StockTransactionResult<StockTransactionResponse>.Failure("Stock was not found for the specified product.", StockTransactionErrorType.NotFound);
            }

            int quantityAfter = stock.Quantity + command.QuantityDelta;
            if (quantityAfter < 0)
            {
                return StockTransactionResult<StockTransactionResponse>.Failure("Stock quantity must not become negative.");
            }

            StockTransaction transaction = stock.ApplyTransaction(command.Type, command.QuantityDelta, command.Reason, command.CreatedById);
            _ = dbContext.StockTransactions.Add(transaction);
            _ = await dbContext.SaveChangesAsync(cancellationToken);

            return StockTransactionResult<StockTransactionResponse>.Success(ToResponse(transaction));
        }

        private static string? ValidateCommand(CreateStockTransactionCommand command)
        {
            if (!Enum.IsDefined(command.Type))
            {
                return "Type must be a defined stock transaction type.";
            }

            if (command.QuantityDelta == 0)
            {
                return "Quantity delta must not be zero.";
            }

            if (command.Type == StockTransactionType.Inbound && command.QuantityDelta < 0)
            {
                return "Inbound transactions must increase stock quantity.";
            }

            if (command.Type == StockTransactionType.Outbound && command.QuantityDelta > 0)
            {
                return "Outbound transactions must decrease stock quantity.";
            }

            return command.Reason?.Length > 255 ? "Reason must be 255 characters or fewer." : null;
        }

        private static StockTransactionResponse ToResponse(StockTransaction transaction)
        {
            return new(
                transaction.Id,
                transaction.ProductId,
                transaction.StockId,
                transaction.Type,
                transaction.QuantityDelta,
                transaction.QuantityAfter,
                transaction.Reason,
                transaction.CreatedById,
                transaction.CreatedAt);
        }
    }
}
