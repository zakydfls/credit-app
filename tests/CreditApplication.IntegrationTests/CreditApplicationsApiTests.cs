using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CreditApplication.IntegrationTests;

public sealed class CreditApplicationsApiTests(
    CreditApplicationApiFactory factory) : IClassFixture<CreditApplicationApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateThenRead_ReturnsApplicationInSuccessEnvelope()
    {
        using var create = await _client.PostAsJsonAsync("/api/credit-applications", new
        {
            plafon = 100_000_000m,
            bunga = 12m,
            tenor = 60
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createdJson = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        Assert.True(createdJson.RootElement.GetProperty("success").GetBoolean());
        var id = createdJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        Assert.Equal(2_224_444.77m,
            createdJson.RootElement.GetProperty("data").GetProperty("angsuran").GetDecimal());

        using var read = await _client.GetAsync($"/api/credit-applications/{id}");
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);

        using var list = await _client.GetAsync("/api/credit-applications?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var listJson = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.True(listJson.RootElement.GetProperty("data")
            .GetProperty("totalCount").GetInt32() > 0);
    }

    [Fact]
    public async Task Update_RecalculatesInstallmentFromUpdatedTerms()
    {
        using var create = await _client.PostAsJsonAsync("/api/credit-applications", new
        {
            plafon = 100_000_000m,
            bunga = 12m,
            tenor = 60
        });
        using var createdJson = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = createdJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        using var update = await _client.PutAsJsonAsync($"/api/credit-applications/{id}", new
        {
            plafon = 50_000_000m,
            bunga = 24m,
            tenor = 12
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        using var updatedJson = JsonDocument.Parse(await update.Content.ReadAsStringAsync());
        Assert.Equal(4_727_979.83m,
            updatedJson.RootElement.GetProperty("data").GetProperty("angsuran").GetDecimal());
    }

    [Fact]
    public async Task DeleteThenRead_ReturnsNotFound()
    {
        using var create = await _client.PostAsJsonAsync("/api/credit-applications", new
        {
            plafon = 10_000_000m,
            bunga = 12m,
            tenor = 12
        });
        using var createdJson = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = createdJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        using var delete = await _client.DeleteAsync($"/api/credit-applications/{id}");
        using var read = await _client.GetAsync($"/api/credit-applications/{id}");

        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
    }

    [Fact]
    public async Task CalculateInstallment_ReturnsQuestionExample()
    {
        using var response = await _client.PostAsJsonAsync("/api/installments/calculate", new
        {
            plafon = 100_000_000m,
            bunga = 12m,
            tenor = 60
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(2_224_444.77m,
            json.RootElement.GetProperty("data").GetProperty("angsuran").GetDecimal());

        using var longTenor = await _client.PostAsJsonAsync("/api/installments/calculate", new
        {
            plafon = 100_000_000m, bunga = 12m, tenor = int.MaxValue
        });
        Assert.Equal(HttpStatusCode.OK, longTenor.StatusCode);
        using var longTenorJson = JsonDocument.Parse(await longTenor.Content.ReadAsStringAsync());
        Assert.Equal(1_000_000m,
            longTenorJson.RootElement.GetProperty("data").GetProperty("angsuran").GetDecimal());
    }

    [Fact]
    public async Task CalculateInstallment_WithInvalidInput_ReturnsValidationError()
    {
        using var response = await _client.PostAsJsonAsync("/api/installments/calculate", new
        {
            plafon = 0m,
            bunga = 12m,
            tenor = 60
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.GetProperty("success").GetBoolean());
        Assert.True(json.RootElement.GetProperty("errors").TryGetProperty("Plafon", out _));
    }
}
