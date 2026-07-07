namespace AspNetNextApp.Api.Services.Products;

public enum ProductErrorType
{
    Validation,
    NotFound,
    Conflict,
}

public sealed record ProductResult<T>(T? Value, bool IsSuccess, string? Error = null, ProductErrorType? ErrorType = null)
{
    public static ProductResult<T> Success(T value) => new(value, true);

    public static ProductResult<T> Failure(string error, ProductErrorType errorType = ProductErrorType.Validation) =>
        new(default, false, error, errorType);
}
