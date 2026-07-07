using AspNetNextApp.Api.Entities;

namespace AspNetNextApp.Api.Services.Accounts
{
    public interface IAccountAuthenticationService
    {
        Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);

        Task<AccountRegistrationResult> RegisterAsync(
            string email,
            string password,
            string name,
            CancellationToken cancellationToken = default);
    }
}
