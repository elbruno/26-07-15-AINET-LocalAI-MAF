using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Shared.Contracts;
using Xunit;

namespace Analytics.Tests;

public sealed class UsageApiTests : IClassFixture<AnalyticsApiFactory>, IDisposable
{
    private readonly HttpClient _client;

    public UsageApiTests(AnalyticsApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Ingest_ReturnsValidationErrors_ForInvalidPayload()
    {
        var invalidPayload = new UsageRecord
        {
            ProxyName = "",
            Backend = "",
            Model = "",
            LatencyMs = -1,
            PromptTokens = -1,
            CompletionTokens = 0,
            TotalTokens = 0
        };

        using var response = await _client.PostAsJsonAsync("/api/usage/ingest", invalidPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(validation);
        var errors = validation.Errors;
        Assert.Contains("proxyName", errors.Keys);
        Assert.Contains("backend", errors.Keys);
        Assert.Contains("model", errors.Keys);
        Assert.Contains("latencyMs", errors.Keys);
        Assert.Contains("tokens", errors.Keys);
    }

    [Fact]
    public async Task IngestQueryAndSummary_ReturnExpectedBaselineBehavior()
    {
        var record1 = new UsageRecord
        {
            RecordedAtUtc = DateTimeOffset.Parse("2026-07-14T10:00:00Z"),
            ProxyName = "local-proxy",
            Backend = "ollama",
            Model = "phi",
            PromptTokens = 10,
            CompletionTokens = 15,
            TotalTokens = 25,
            LatencyMs = 200,
            Success = true
        };

        var record2 = new UsageRecord
        {
            RecordedAtUtc = DateTimeOffset.Parse("2026-07-14T10:01:00Z"),
            ProxyName = "local-proxy",
            Backend = "foundry-local",
            Model = "phi",
            PromptTokens = 20,
            CompletionTokens = 30,
            TotalTokens = 50,
            LatencyMs = 400,
            Success = false
        };

        await PostRecordAsync(record1);
        await PostRecordAsync(record2);

        using var filteredResponse = await _client.GetAsync("/api/usage?model=phi&success=true&limit=1");
        var filteredBody = await filteredResponse.Content.ReadAsStringAsync();
        Assert.True(filteredResponse.IsSuccessStatusCode, $"Expected usage query to succeed but got {(int)filteredResponse.StatusCode}: {filteredBody}");
        var filtered = await filteredResponse.Content.ReadFromJsonAsync<UsageRecordsResponse>();
        Assert.NotNull(filtered);
        Assert.Equal(1, filtered.TotalCount);
        Assert.Single(filtered.Items);
        Assert.Equal(record1.Backend, filtered.Items[0].Backend);

        using var summaryResponse = await _client.GetAsync("/api/usage/summary");
        var summaryBody = await summaryResponse.Content.ReadAsStringAsync();
        Assert.True(summaryResponse.IsSuccessStatusCode, $"Expected summary query to succeed but got {(int)summaryResponse.StatusCode}: {summaryBody}");
        var summary = await summaryResponse.Content.ReadFromJsonAsync<UsageDashboardSummary>();
        Assert.NotNull(summary);
        Assert.Equal(2, summary.TotalRequests);
        Assert.Equal(1, summary.SuccessfulRequests);
        Assert.Equal(1, summary.FailedRequests);
        Assert.Equal(75, summary.TotalTokens);
        Assert.Equal(300d, summary.AverageLatencyMs);
    }

    private async Task PostRecordAsync(UsageRecord record)
    {
        using var response = await _client.PostAsJsonAsync("/api/usage/ingest", record);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

public sealed class AnalyticsApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _dbPath = Path.Combine(AppContext.BaseDirectory, $"analytics-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Analytics"] = $"Data Source={_dbPath}"
            });
        });
    }

    public new void Dispose()
    {
        base.Dispose();
        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch (IOException)
        {
        }
    }
}
