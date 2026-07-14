using System.Text.Json;
using Shared.Contracts;
using Xunit;

namespace Shared.Tests;

public sealed class UsageContractsTests
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void UsageRecord_InitializesExpectedDefaults()
    {
        var record = new UsageRecord();

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(string.Empty, record.ProxyName);
        Assert.Equal(string.Empty, record.Backend);
        Assert.Equal(string.Empty, record.Model);
        Assert.True(record.Success);
        Assert.True(record.RecordedAtUtc > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void UsageRecord_RoundTripsWithWebJsonSerializer()
    {
        var expected = new UsageRecord
        {
            Id = Guid.NewGuid(),
            RecordedAtUtc = DateTimeOffset.Parse("2026-07-14T12:00:00Z"),
            ProxyName = "proxy-1",
            Backend = "ollama",
            Model = "phi-4-mini",
            PromptTokens = 10,
            CompletionTokens = 5,
            TotalTokens = 15,
            LatencyMs = 120,
            Success = false,
            HttpStatusCode = 500,
            ErrorType = "upstream",
            ErrorMessage = "boom"
        };

        var json = JsonSerializer.Serialize(expected, WebJson);
        var actual = JsonSerializer.Deserialize<UsageRecord>(json, WebJson);

        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.RecordedAtUtc, actual.RecordedAtUtc);
        Assert.Equal(expected.ProxyName, actual.ProxyName);
        Assert.Equal(expected.Backend, actual.Backend);
        Assert.Equal(expected.Model, actual.Model);
        Assert.Equal(expected.PromptTokens, actual.PromptTokens);
        Assert.Equal(expected.CompletionTokens, actual.CompletionTokens);
        Assert.Equal(expected.TotalTokens, actual.TotalTokens);
        Assert.Equal(expected.LatencyMs, actual.LatencyMs);
        Assert.Equal(expected.Success, actual.Success);
        Assert.Equal(expected.HttpStatusCode, actual.HttpStatusCode);
        Assert.Equal(expected.ErrorType, actual.ErrorType);
        Assert.Equal(expected.ErrorMessage, actual.ErrorMessage);
    }

    [Fact]
    public void UsageRecordsQuery_DefaultsToExpectedLimit()
    {
        var query = new UsageRecordsQuery();

        Assert.Equal(50, query.Limit);
        Assert.Null(query.Backend);
        Assert.Null(query.Model);
        Assert.Null(query.Success);
    }
}
