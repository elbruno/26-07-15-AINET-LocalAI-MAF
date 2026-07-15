namespace Shared.Contracts;

public interface IUsageClient
{
    Task ReportAsync(UsageRecord usageRecord, CancellationToken cancellationToken = default);
}
