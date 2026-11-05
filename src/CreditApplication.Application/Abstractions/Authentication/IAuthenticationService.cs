using CreditApplication.Application.Auth;

namespace CreditApplication.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<RegisterUserResult?> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken);

    Task<AuthenticationResult?> AuthenticateAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken);
}
