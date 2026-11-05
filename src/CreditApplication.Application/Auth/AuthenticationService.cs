using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Abstractions.Persistence;
using CreditApplication.Domain.Entities;

namespace CreditApplication.Application.Auth;

public sealed class AuthenticationService(
    IUserRepository users,
    IPasswordService passwords,
    ITokenService tokens) : IAuthenticationService
{
    public async Task<RegisterUserResult?> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User(command.Username);
        if (await users.FindByNormalizedUsernameAsync(user.NormalizedUsername, cancellationToken) is not null)
            return null;

        user.SetPasswordHash(passwords.Hash(user, command.Password));
        if (!await users.AddAsync(user, cancellationToken))
            return null;

        return new RegisterUserResult(user.Id, user.Username);
    }

    public async Task<AuthenticationResult?> AuthenticateAsync(AuthenticateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await users.FindByNormalizedUsernameAsync(command.Username.ToUpperInvariant(), cancellationToken);
        if (user is null)
            return null;

        var verification = passwords.Verify(user, command.Password);
        if (verification == PasswordCheckResult.Failed)
            return null;

        if (verification == PasswordCheckResult.SuccessRehashNeeded)
        {
            user.SetPasswordHash(passwords.Hash(user, command.Password));
            await users.SaveChangesAsync(cancellationToken);
        }

        return tokens.CreateToken(user);
    }
}
