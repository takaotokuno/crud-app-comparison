namespace AspNetNextApp.Api.Services.StockTransactions
{
    public enum StockTransactionErrorType
    {
        Validation,
        NotFound,
    }

    public sealed record StockTransactionResult<T>(T? Value, bool IsSuccess, string? Error = null, StockTransactionErrorType? ErrorType = null)
    {
        public static StockTransactionResult<T> Success(T value)
        {
            return new(value, true);
        }

        public static StockTransactionResult<T> Failure(string error, StockTransactionErrorType errorType = StockTransactionErrorType.Validation)
        {
            return new(default, false, error, errorType);
        }
    }
}
