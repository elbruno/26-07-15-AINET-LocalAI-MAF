namespace Shared.Contracts;

public sealed class UsageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset RecordedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string ProxyName { get; set; } = string.Empty;
    public string Backend { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    public long LatencyMs { get; set; }
    public bool Success { get; set; } = true;
    public int? HttpStatusCode { get; set; }
    public string? ErrorType { get; set; }
    public string? ErrorMessage { get; set; }
}
