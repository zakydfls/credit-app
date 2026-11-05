using System.Text.Encodings.Web;
using System.Security.Claims;
using CreditApplication.Application.Common;
using CreditApplication.Application.CreditApplications;
using CreditApplication.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CreditApplication.IntegrationTests;

public sealed class CreditApplicationApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CreditDatabase"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["SeedDemoUser"] = "false",
                ["Jwt:Issuer"] = "integration-tests",
                ["Jwt:Audience"] = "integration-tests",
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-bytes-long"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICreditApplicationService>();
            services.AddSingleton<FakeCreditApplicationService>();
            services.AddSingleton<ICreditApplicationService>(provider =>
                provider.GetRequiredService<FakeCreditApplicationService>());

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Test";
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                "Test", _ => { });
        });
    }
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "b68162df-2870-4ba5-9716-39a2aceb9bb6")],
            Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

internal sealed class FakeCreditApplicationService : ICreditApplicationService
{
    private readonly Dictionary<Guid, CreditApplicationDto> _applications = [];

    private static decimal CalculateInstallment(decimal plafon, decimal bunga, int tenor) =>
        InstallmentCalculator.Calculate(plafon, bunga, tenor);

    public Task<CreditApplicationDto> CreateAsync(
        Guid userId,
        CreateCreditApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var application = new CreditApplicationDto(
            Guid.NewGuid(), command.Plafon, command.Bunga, command.Tenor,
            CalculateInstallment(command.Plafon, command.Bunga, command.Tenor), now, now, userId);
        _applications.Add(application.Id, application);
        return Task.FromResult(application);
    }

    public Task<CreditApplicationDto?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken)
    {
        _applications.TryGetValue(id, out var application);
        return Task.FromResult(application?.UserId == userId ? application : null);
    }

    public Task<PagedResult<CreditApplicationDto>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var all = _applications.Values
            .Where(application => application.UserId == userId)
            .OrderByDescending(application => application.CreatedAt)
            .ThenBy(application => application.Id)
            .ToArray();
        return Task.FromResult(new PagedResult<CreditApplicationDto>(
            all.Skip((int)Math.Min(((long)page - 1) * pageSize, int.MaxValue)).Take(pageSize).ToArray(),
            page,
            pageSize,
            all.Length));
    }

    public Task<CreditApplicationDto?> UpdateAsync(
        Guid id,
        Guid userId,
        UpdateCreditApplicationCommand command,
        CancellationToken cancellationToken)
    {
        if (!_applications.TryGetValue(id, out var application) || application.UserId != userId)
            return Task.FromResult<CreditApplicationDto?>(null);

        var updated = application with
        {
            Plafon = command.Plafon,
            Bunga = command.Bunga,
            Tenor = command.Tenor,
            Angsuran = CalculateInstallment(command.Plafon, command.Bunga, command.Tenor),
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _applications[id] = updated;
        return Task.FromResult<CreditApplicationDto?>(updated);
    }

    public Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(_applications.TryGetValue(id, out var application) &&
            application.UserId == userId && _applications.Remove(id));
}
