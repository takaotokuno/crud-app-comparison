namespace AspNetNextApp.Api.Services.Products;

public sealed record ProductResult<T>(T? Value, bool IsSuccess, string? Error = null)
{
    public static ProductResult<T> Success(T value) => new(value, true);

    public static ProductResult<T> Failure(string error) => new(default, false, error);
}
