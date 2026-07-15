# 09-analytics-aspire

This is the repo's companion Aspire sample: a simple analytics dashboard and API that now lives alongside the Foundry Local demos under `samples\`.

## What it contains

- `AppHost` — Aspire orchestration
- `analytics\Analytics.Api` — Minimal API + SQLite persistence
- `analytics\Analytics.Web` — Blazor dashboard
- `Shared` — shared contracts

## Run

From the repository root:

```powershell
dotnet restore dotnet-local-ai.slnx
dotnet build dotnet-local-ai.slnx
aspire start --apphost samples\09-analytics-aspire\AppHost\AppHost.csproj
```

## Direct run

```powershell
dotnet run --project samples\09-analytics-aspire\analytics\Analytics.Api\Analytics.Api.csproj
dotnet run --project samples\09-analytics-aspire\analytics\Analytics.Web\Analytics.Web.csproj
```
