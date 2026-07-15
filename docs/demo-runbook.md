# Demo Runbook (Current Repository State)

This runbook walks from clone to a working local analytics demo.

## 1) Restore and build

From repo root:

```powershell
dotnet restore dotnet-local-ai.slnx
dotnet build dotnet-local-ai.slnx
```

Expected outcome:
- Restore and build complete successfully.

## 2) Start the full graph with AppHost

```powershell
aspire start --apphost samples\09-analytics-aspire\AppHost\AppHost.csproj
```

Expected outcome:
- Aspire CLI reports startup details, including the dashboard endpoint.
- `analytics-api` and `analytics-web` resources show as running.

## 3) Open the dashboard UI

- In Aspire dashboard, open the `analytics-web` endpoint.
- If needed for standalone mode, use `https://localhost:7025/`.

Expected outcome:
- You see **Usage Dashboard** with KPI cards and filters.
- On a fresh DB, table is empty (this is expected).

## 4) Send sample usage records to the API

From a new terminal:

```powershell
curl -k -X POST https://localhost:7013/api/usage/ingest `
  -H "Content-Type: application/json" `
  -d "{\"proxyName\":\"OllamaProxy\",\"backend\":\"ollama\",\"model\":\"llama3.2:1b\",\"promptTokens\":40,\"completionTokens\":96,\"totalTokens\":136,\"latencyMs\":210,\"success\":true}"
```

```powershell
curl -k -X POST https://localhost:7013/api/usage/ingest `
  -H "Content-Type: application/json" `
  -d "{\"proxyName\":\"FoundryLocalProxy\",\"backend\":\"foundrylocal\",\"model\":\"qwen2.5-0.5b\",\"promptTokens\":28,\"completionTokens\":52,\"totalTokens\":80,\"latencyMs\":143,\"success\":false,\"httpStatusCode\":502,\"errorType\":\"backend_unavailable\",\"errorMessage\":\"Model service offline\"}"
```

Expected outcome:
- API returns `201 Created` for valid records.

## 5) Verify API summary

```powershell
curl -k https://localhost:7013/api/usage/summary
```

Expected outcome:
- JSON summary with totals, success/failure counts, total tokens, and average latency.

## 6) Verify UI reflects activity

- Refresh the dashboard page.
- Optionally filter by provider/model.

Expected outcome:
- KPI cards update.
- Recent requests table shows inserted records.

---

## Optional: Run services directly (no AppHost)

Terminal 1:

```powershell
dotnet run --project samples\09-analytics-aspire\analytics\Analytics.Api\Analytics.Api.csproj
```

Terminal 2:

```powershell
dotnet run --project samples\09-analytics-aspire\analytics\Analytics.Web\Analytics.Web.csproj
```

Then browse `https://localhost:7025/`.

---

## Quick checks

- API health: `https://localhost:7013/health`
- API root: `https://localhost:7013/`
- Usage list: `https://localhost:7013/api/usage?limit=50`
