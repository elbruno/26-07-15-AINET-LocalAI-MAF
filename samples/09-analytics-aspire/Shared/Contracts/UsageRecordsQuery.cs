namespace Shared.Contracts;

public sealed class UsageRecordsQuery
{
    public int Limit { get; init; } = 50;
    public string? Backend { get; init; }
    public string? Model { get; init; }
    public bool? Success { get; init; }
}
