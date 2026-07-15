using System.Net.Http.Json;
using Shared.Contracts;

namespace Analytics.Web.Services;

public sealed class AnalyticsApiClient(HttpClient httpClient)
{
    public async Task<UsageDashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("/api/usage/summary", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Unable to load dashboard summary.");
        }

        return await response.Content.ReadFromJsonAsync<UsageDashboardSummary>(cancellationToken)
            ?? throw new HttpRequestException("Dashboard summary payload was empty.");
    }

    public async Task<UsageRecordsResponse> GetUsageRecordsAsync(UsageRecordsQuery query, CancellationToken cancellationToken = default)
    {
        var requestUri = BuildUsageQuery(query);
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Unable to load usage records.");
        }

        return await response.Content.ReadFromJsonAsync<UsageRecordsResponse>(cancellationToken)
            ?? throw new HttpRequestException("Usage records payload was empty.");
    }

    private static string BuildUsageQuery(UsageRecordsQuery query)
    {
        var queryParts = new List<string>
        {
            $"limit={Math.Clamp(query.Limit, 1, 200)}"
        };

        if (!string.IsNullOrWhiteSpace(query.Backend))
        {
            queryParts.Add($"backend={Uri.EscapeDataString(query.Backend)}");
        }

        if (!string.IsNullOrWhiteSpace(query.Model))
        {
            queryParts.Add($"model={Uri.EscapeDataString(query.Model)}");
        }

        if (query.Success.HasValue)
        {
            queryParts.Add($"success={query.Success.Value.ToString().ToLowerInvariant()}");
        }

        return $"/api/usage?{string.Join("&", queryParts)}";
    }
}
