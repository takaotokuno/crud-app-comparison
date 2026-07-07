using AspNetNextApp.Api.Entities;

namespace AspNetNextApp.Api.Services.Accounts
{
    public interface IAccountAuthenticationService
    {
        Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    }
}
