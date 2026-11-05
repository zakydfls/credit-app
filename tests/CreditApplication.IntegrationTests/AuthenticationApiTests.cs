using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditApplication.Infrastructure.Persistence;
using CreditApplication.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace CreditApplication.IntegrationTests;

public sealed class AuthenticationApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationApiTests()
    {
        _factory = CreateFactory(Guid.NewGuid().ToString(), false, "Development");
        _client = _factory.CreateClient();
    }

    private static WebApplicationFactory<Program> CreateFactory(string databaseName, bool seedDemoUser, string environment, bool enableSwagger = false) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("Jwt:SigningKey", "test-signing-key-at-least-32-characters-long");
            builder.UseSetting("ConnectionStrings:CreditDatabase", "Host=localhost;Database=test;Username=test;Password=test");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CreditDatabase"] = "Host=localhost;Database=test;Username=test;Password=test",
                    ["SeedDemoUser"] = seedDemoUser.ToString(),
                    ["Swagger:Enabled"] = enableSwagger.ToString(),
                    ["Jwt:Issuer"] = "test-issuer",
                    ["Jwt:Audience"] = "test-audience",
                    ["Jwt:SigningKey"] = "test-signing-key-at-least-32-characters-long"
                }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<CreditApplicationDbContext>>();
                services.RemoveAll<CreditApplicationDbContext>();
                services.AddDbContext<CreditApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters.ValidIssuer = "test-issuer";
                    options.TokenValidationParameters.ValidAudience = "test-audience";
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("test-signing-key-at-least-32-characters-long"));
                });
            });
        });

    [Theory]
    [InlineData(false, HttpStatusCode.NotFound)]
    [InlineData(true, HttpStatusCode.OK)]
    public async Task ProductionSwagger_FollowsConfiguration(bool enabled, HttpStatusCode expectedStatus)
    {
        using var factory = CreateFactory(Guid.NewGuid().ToString(), false, "Production", enabled);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(expectedStatus, response.StatusCode);
        using var uiResponse = await client.GetAsync("/swagger/index.html");
        Assert.Equal(expectedStatus, uiResponse.StatusCode);
    }

    [Fact]
    public async Task DevelopmentStartup_SeedsDemoUser_AndRepeatDoesNotOverwritePassword()
    {
        using var factory = CreateFactory(Guid.NewGuid().ToString(), true, "Development");
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreditApplicationDbContext>();
        var seeded = await db.Users.SingleAsync(u => u.NormalizedUsername == "SKYWORX");
        var originalHash = seeded.PasswordHash;
        Assert.NotEqual("demo12345", originalHash);

        await scope.ServiceProvider.GetRequiredService<DemoUserSeeder>().SeedAsync();
        Assert.Equal(1, await db.Users.CountAsync(u => u.NormalizedUsername == "SKYWORX"));
        Assert.Equal(originalHash, (await db.Users.SingleAsync(u => u.NormalizedUsername == "SKYWORX")).PasswordHash);

        using var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "skyworx", password = "demo12345" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task ProductionStartup_DoesNotSeedDemoUser_EvenWhenEnabled()
    {
        using var factory = CreateFactory(Guid.NewGuid().ToString(), true, "Production");
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CreditApplicationDbContext>();
        Assert.Empty(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task Seeder_WhenUsernameAlreadyExists_DoesNotReplaceExistingPassword()
    {
        using var register = await _client.PostAsJsonAsync("/api/auth/register",
            new { username = "skyworx", password = "existing-password" });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DemoUserSeeder>().SeedAsync();

        using var existingLogin = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "skyworx", password = "existing-password" });
        using var demoLogin = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "skyworx", password = "demo12345" });
        Assert.Equal(HttpStatusCode.OK, existingLogin.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, demoLogin.StatusCode);
    }

    [Fact]
    public async Task RegisterAndLogin_StoresHashAndReturnsUsableJwt()
    {
        var credentials = new { username = "new.user", password = "password123" };
        using var register = await _client.PostAsJsonAsync("/api/auth/register", credentials);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        using var registrationJson = JsonDocument.Parse(await register.Content.ReadAsStringAsync());
        Assert.Equal("new.user", registrationJson.RootElement.GetProperty("data").GetProperty("username").GetString());
        Assert.True(registrationJson.RootElement.GetProperty("data").TryGetProperty("id", out _));
        Assert.True(registrationJson.RootElement.GetProperty("success").GetBoolean());
        Assert.False(string.IsNullOrEmpty(registrationJson.RootElement.GetProperty("requestId").GetString()));
        Assert.DoesNotContain("password123", registrationJson.RootElement.ToString());

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CreditApplicationDbContext>();
            var user = await db.Users.SingleAsync();
            Assert.NotEqual(credentials.password, user.PasswordHash);
            Assert.DoesNotContain(credentials.password, user.PasswordHash);
        }

        using var login = await _client.PostAsJsonAsync("/api/auth/login", credentials);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var protectedRequest = await _client.PostAsJsonAsync("/api/installments/calculate", new
        {
            plafon = 100_000_000m, bunga = 12m, tenor = 60
        });
        Assert.Equal(HttpStatusCode.OK, protectedRequest.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUsernameIgnoringCase_ReturnsConflict()
    {
        using var first = await _client.PostAsJsonAsync("/api/auth/register", new { username = "TestUser", password = "password123" });
        using var duplicate = await _client.PostAsJsonAsync("/api/auth/register", new { username = "testuser", password = "another123" });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var register = await _client.PostAsJsonAsync("/api/auth/register", new { username = "testuser", password = "password123" });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        using var login = await _client.PostAsJsonAsync("/api/auth/login", new { username = "testuser", password = "incorrect123" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidInput_ReturnsBadRequest()
    {
        using var response = await _client.PostAsJsonAsync("/api/auth/register", new { username = "a", password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreditApplications_AreRestrictedToTheirOwner()
    {
        var aliceToken = await RegisterAndLogin("alice");
        var bobToken = await RegisterAndLogin("bobby");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aliceToken);
        using var create = await _client.PostAsJsonAsync("/api/credit-applications", new
        {
            plafon = 100_000_000m, bunga = 12m, tenor = 60,
            userId = Guid.NewGuid()
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var json = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = json.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        var ownerId = json.RootElement.GetProperty("data").GetProperty("userId").GetGuid();
        Assert.True(json.RootElement.GetProperty("data").TryGetProperty("createdAt", out _));
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CreditApplicationDbContext>();
            Assert.Equal((await db.Users.SingleAsync(u => u.Username == "alice")).Id, ownerId);
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bobToken);
        using var list = await _client.GetAsync("/api/credit-applications");
        using var listJson = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(0, listJson.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        using var read = await _client.GetAsync($"/api/credit-applications/{id}");
        using var update = await _client.PutAsJsonAsync($"/api/credit-applications/{id}",
            new { plafon = 50_000_000m, bunga = 12m, tenor = 12 });
        using var delete = await _client.DeleteAsync($"/api/credit-applications/{id}");
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aliceToken);
        using var ownRead = await _client.GetAsync($"/api/credit-applications/{id}");
        Assert.Equal(HttpStatusCode.OK, ownRead.StatusCode);
        using var lastPage = await _client.GetAsync("/api/credit-applications?page=2147483647&pageSize=100");
        Assert.Equal(HttpStatusCode.OK, lastPage.StatusCode);
        using var pageJson = JsonDocument.Parse(await lastPage.Content.ReadAsStringAsync());
        Assert.Equal(1, pageJson.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        Assert.Equal(1, pageJson.RootElement.GetProperty("data").GetProperty("totalPages").GetInt32());
        Assert.Empty(pageJson.RootElement.GetProperty("data").GetProperty("items").EnumerateArray());
        using var ownUpdate = await _client.PutAsJsonAsync($"/api/credit-applications/{id}",
            new { plafon = 50_000_000m, bunga = 12m, tenor = 12 });
        Assert.Equal(HttpStatusCode.OK, ownUpdate.StatusCode);
        using var ownDelete = await _client.DeleteAsync($"/api/credit-applications/{id}");
        Assert.Equal(HttpStatusCode.OK, ownDelete.StatusCode);
    }

    [Fact]
    public async Task CreditApplication_WhenInstallmentExceedsDatabaseRange_ReturnsValidationError()
    {
        var token = await RegisterAndLogin("rangeuser");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await _client.PostAsJsonAsync("/api/credit-applications", new
        {
            plafon = 9_999_999_999_999_999.99m, bunga = 12m, tenor = 1
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("Plafon", out _));

        using var tiny = await _client.PostAsJsonAsync("/api/credit-applications", new
        {
            plafon = 0.01m, bunga = 0.01m, tenor = int.MaxValue
        });
        Assert.Equal(HttpStatusCode.BadRequest, tiny.StatusCode);
    }

    [Fact]
    public async Task LegacyToken_WithUsernameSubject_IsRejected()
    {
        var token = new JwtSecurityToken(
            issuer: "test-issuer", audience: "test-audience",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, "alice")],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-at-least-32-characters-long")),
                SecurityAlgorithms.HmacSha256));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        using var response = await _client.GetAsync("/api/credit-applications");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> RegisterAndLogin(string username)
    {
        var credentials = new { username, password = "password123" };
        using var register = await _client.PostAsJsonAsync("/api/auth/register", credentials);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        using var login = await _client.PostAsJsonAsync("/api/auth/login", credentials);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
