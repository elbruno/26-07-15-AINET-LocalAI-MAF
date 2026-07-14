# Local AI with .NET (Current Scaffold)

This repo currently contains the analytics slice of the **Local AI with .NET** demo:

- `src\AppHost` (.NET Aspire orchestration)
- `src\analytics\Analytics.Api` (Minimal API + SQLite)
- `src\analytics\Analytics.Web` (Blazor dashboard)
- `src\Shared` (shared contracts)

> Full session vision is in `docs\Local-AI-with-dotNET-PRD.md`.

## Prerequisites

- .NET SDK `10.0.301` (see `global.json`)
- Git
- (Recommended) Aspire workload/tools for the best AppHost experience:
  - `dotnet workload install aspire`

## Quick start (AppHost, end-to-end)

From repository root:

```powershell
dotnet restore dotnet-local-ai.slnx
dotnet build dotnet-local-ai.slnx
dotnet run --project src\AppHost\AppHost.csproj
```

What to expect:

- Aspire AppHost starts and opens dashboard (default profile uses `https://localhost:17116`).
- `analytics-api` and `analytics-web` start as part of the AppHost graph.
- Open the `analytics-web` resource from the Aspire dashboard to view the dashboard UI.

## Run services directly (without AppHost)

### 1) API

```powershell
dotnet run --project src\analytics\Analytics.Api\Analytics.Api.csproj
```

Expected local endpoints:

- `https://localhost:7013/`
- `https://localhost:7013/health`
- `https://localhost:7013/api/usage`
- `https://localhost:7013/api/usage/summary`

### 2) Web

In a second terminal:

```powershell
dotnet run --project src\analytics\Analytics.Web\Analytics.Web.csproj
```

Expected UI URL:

- `https://localhost:7025/`

## Seed demo data (optional)

Post one usage record:

```powershell
curl -k -X POST https://localhost:7013/api/usage/ingest `
  -H "Content-Type: application/json" `
  -d "{\"proxyName\":\"OllamaProxy\",\"backend\":\"ollama\",\"model\":\"llama3.2:1b\",\"promptTokens\":32,\"completionTokens\":76,\"totalTokens\":108,\"latencyMs\":184,\"success\":true}"
```

Then open `https://localhost:7025/` and confirm KPI cards and the recent requests table show data.

## Troubleshooting

- **SDK not found / wrong version:** run `dotnet --info` and ensure SDK `10.0.301` is installed.
- **HTTPS certificate warning:** run `dotnet dev-certs https --trust`.
- **Port already in use:** stop conflicting process or launch with another profile/port.
- **Dashboard is empty:** confirm API is running and reachable at the configured base URL.

## More docs

- Step-by-step runbook: `docs\demo-runbook.md`
- Demo narrative/story flow: `docs\demo-story-flow.md`
- Product requirements: `docs\Local-AI-with-dotNET-PRD.md`
