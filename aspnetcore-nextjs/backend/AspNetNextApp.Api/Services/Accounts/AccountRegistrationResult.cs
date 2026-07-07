using AspNetNextApp.Api.Entities;

namespace AspNetNextApp.Api.Services.Accounts
{
    public sealed record AccountRegistrationResult(User? User, string? ErrorMessage)
    {
        public bool Succeeded => User is not null;

        public static AccountRegistrationResult Success(User user)
        {
            return new(user, null);
        }

        public static AccountRegistrationResult Failure(string errorMessage)
        {
            return new(null, errorMessage);
        }
    }
}
