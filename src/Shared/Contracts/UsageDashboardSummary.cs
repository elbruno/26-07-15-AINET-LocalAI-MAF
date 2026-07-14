namespace Shared.Contracts;

public sealed class UsageDashboardSummary
{
    public required int TotalRequests { get; init; }
    public required int SuccessfulRequests { get; init; }
    public required int FailedRequests { get; init; }
    public required long TotalTokens { get; init; }
    public required double AverageLatencyMs { get; init; }
}
