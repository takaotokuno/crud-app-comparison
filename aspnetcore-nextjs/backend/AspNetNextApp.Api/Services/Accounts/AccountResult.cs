namespace AspNetNextApp.Api.Services.Accounts
{
    public enum AccountErrorType
    {
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
    }

    public sealed record AccountResult<T>(T? Value, bool IsSuccess, string? Error = null, AccountErrorType? ErrorType = null)
    {
        public static AccountResult<T> Success(T value)
        {
            return new(value, true);
        }

        public static AccountResult<T> Failure(string error, AccountErrorType errorType = AccountErrorType.Validation)
        {
            return new(default, false, error, errorType);
        }
    }
}
