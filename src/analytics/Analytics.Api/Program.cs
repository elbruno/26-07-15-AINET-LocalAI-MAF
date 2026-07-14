using Analytics.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AnalyticsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Analytics") ?? "Data Source=analytics.db";
    options.UseSqlite(connectionString);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { service = "Analytics.Api", status = "ready" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

var usageGroup = app.MapGroup("/api/usage");

usageGroup.MapPost("/ingest", async ([FromBody] UsageRecord payload, AnalyticsDbContext dbContext, CancellationToken cancellationToken) =>
{
    var errors = ValidateUsageRecord(payload);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    payload.Id = payload.Id == Guid.Empty ? Guid.NewGuid() : payload.Id;
    payload.RecordedAtUtc = payload.RecordedAtUtc == default ? DateTimeOffset.UtcNow : payload.RecordedAtUtc;

    try
    {
        dbContext.UsageRecords.Add(payload);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/usage/{payload.Id}", payload);
    }
    catch (DbUpdateException)
    {
        return Results.Problem(
            title: "Unable to persist usage record",
            detail: "The usage record could not be saved.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("IngestUsageRecord")
.Produces<UsageRecord>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status500InternalServerError);

usageGroup.MapGet("", async ([AsParameters] UsageRecordsQuery query, AnalyticsDbContext dbContext, CancellationToken cancellationToken) =>
{
    var safeLimit = Math.Clamp(query.Limit, 1, 200);

    IQueryable<UsageRecord> recordsQuery = dbContext.UsageRecords.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(query.Backend))
    {
        recordsQuery = recordsQuery.Where(x => x.Backend == query.Backend);
    }

    if (!string.IsNullOrWhiteSpace(query.Model))
    {
        recordsQuery = recordsQuery.Where(x => x.Model == query.Model);
    }

    if (query.Success.HasValue)
    {
        recordsQuery = recordsQuery.Where(x => x.Success == query.Success.Value);
    }

    var totalCount = await recordsQuery.CountAsync(cancellationToken);
    var items = (await recordsQuery.ToListAsync(cancellationToken))
        .OrderByDescending(x => x.RecordedAtUtc)
        .Take(safeLimit)
        .ToList();

    return Results.Ok(new UsageRecordsResponse
    {
        Items = items,
        TotalCount = totalCount
    });
})
.WithName("GetUsageRecords")
.Produces<UsageRecordsResponse>(StatusCodes.Status200OK);

usageGroup.MapGet("/summary", async (AnalyticsDbContext dbContext, CancellationToken cancellationToken) =>
{
    var totalRequests = await dbContext.UsageRecords.CountAsync(cancellationToken);
    var successfulRequests = await dbContext.UsageRecords.CountAsync(x => x.Success, cancellationToken);
    var failedRequests = totalRequests - successfulRequests;

    var totalTokens = await dbContext.UsageRecords.SumAsync(x => (long?)x.TotalTokens, cancellationToken) ?? 0L;
    var averageLatency = await dbContext.UsageRecords.AverageAsync(x => (double?)x.LatencyMs, cancellationToken) ?? 0d;

    return Results.Ok(new UsageDashboardSummary
    {
        TotalRequests = totalRequests,
        SuccessfulRequests = successfulRequests,
        FailedRequests = failedRequests,
        TotalTokens = totalTokens,
        AverageLatencyMs = averageLatency
    });
})
.WithName("GetUsageSummary")
.Produces<UsageDashboardSummary>(StatusCodes.Status200OK);

app.Run();

static Dictionary<string, string[]> ValidateUsageRecord(UsageRecord record)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(record.ProxyName))
    {
        errors["proxyName"] = ["proxyName is required."];
    }

    if (string.IsNullOrWhiteSpace(record.Backend))
    {
        errors["backend"] = ["backend is required."];
    }

    if (string.IsNullOrWhiteSpace(record.Model))
    {
        errors["model"] = ["model is required."];
    }

    if (record.LatencyMs < 0)
    {
        errors["latencyMs"] = ["latencyMs cannot be negative."];
    }

    if (record.TotalTokens < 0 || record.PromptTokens < 0 || record.CompletionTokens < 0)
    {
        errors["tokens"] = ["Token values cannot be negative."];
    }

    return errors;
}

public partial class Program;
