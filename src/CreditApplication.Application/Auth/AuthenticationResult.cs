namespace CreditApplication.Application.Auth;

public sealed record AuthenticationResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
