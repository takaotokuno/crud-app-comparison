namespace AspNetNextApp.Api.Services.Stocks
{
    public enum StockErrorType
    {
        Validation,
        NotFound,
        Conflict,
    }

    public sealed record StockResult<T>(T? Value, bool IsSuccess, string? Error = null, StockErrorType? ErrorType = null)
    {
        public static StockResult<T> Success(T value)
        {
            return new(value, true);
        }

        public static StockResult<T> Failure(string error, StockErrorType errorType = StockErrorType.Validation)
        {
            return new(default, false, error, errorType);
        }
    }
}
