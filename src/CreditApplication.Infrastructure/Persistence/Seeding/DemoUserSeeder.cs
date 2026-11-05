using CreditApplication.Application.Abstractions.Authentication;
using CreditApplication.Application.Auth;
using Microsoft.Extensions.Logging;

namespace CreditApplication.Infrastructure.Persistence.Seeding;

public sealed class DemoUserSeeder(
    IAuthenticationService authenticationService,
    ILogger<DemoUserSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var user = await authenticationService.RegisterAsync(
            new RegisterUserCommand("skyworx", "demo12345"), cancellationToken);

        if (user is null)
            logger.LogInformation("Development demo user already exists; seeding skipped.");
        else
            logger.LogInformation("Development demo user created.");
    }
}
