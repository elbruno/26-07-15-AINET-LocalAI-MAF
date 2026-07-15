namespace Shared.Contracts;

public sealed class UsageRecordsResponse
{
    public required IReadOnlyList<UsageRecord> Items { get; init; }
    public required int TotalCount { get; init; }
}
