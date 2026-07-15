# Demo Story Flow

Use this script to tell a clear “local AI observability” story with the current scaffold.

## Story in 4 acts

## Act 1 — Set context (what this repo is)

- Explain this repository now centers on a **samples-first** Foundry Local story, with the analytics experience moved into `samples\09-analytics-aspire`.
- Point to:
  - `samples\09-analytics-aspire\analytics\Analytics.Api` for ingestion/query APIs
  - `samples\09-analytics-aspire\analytics\Analytics.Web` for dashboard UX
  - `samples\09-analytics-aspire\AppHost` for one-command orchestration

## Act 2 — Start everything with one command

Run:

```powershell
aspire start --apphost samples\09-analytics-aspire\AppHost\AppHost.csproj
```

Talk track:
- “Aspire starts the service graph in the background and gives me one dashboard to operate everything.”
- Show `analytics-api` and `analytics-web` resources in running state.

## Act 3 — Simulate AI traffic

Send sample records into the API (success + failure case):

```powershell
curl -k -X POST https://localhost:7013/api/usage/ingest -H "Content-Type: application/json" -d "{\"proxyName\":\"OllamaProxy\",\"backend\":\"ollama\",\"model\":\"llama3.2:1b\",\"promptTokens\":40,\"completionTokens\":96,\"totalTokens\":136,\"latencyMs\":210,\"success\":true}"
```

```powershell
curl -k -X POST https://localhost:7013/api/usage/ingest -H "Content-Type: application/json" -d "{\"proxyName\":\"FoundryLocalProxy\",\"backend\":\"foundrylocal\",\"model\":\"qwen2.5-0.5b\",\"promptTokens\":28,\"completionTokens\":52,\"totalTokens\":80,\"latencyMs\":143,\"success\":false,\"httpStatusCode\":502,\"errorType\":\"backend_unavailable\",\"errorMessage\":\"Model service offline\"}"
```

Talk track:
- “These records emulate model calls from different backends.”
- “We capture tokens, latency, and success/failure so we can compare local and cloud behavior.”

## Act 4 — Show insights in the dashboard

- Open `analytics-web` from Aspire dashboard.
- Show KPI cards:
  - Total requests
  - Total tokens
  - Avg latency
  - Error rate
- Use filters to isolate provider/model.

Talk track:
- “Same telemetry pattern can be reused as we add the remaining proxy and sample layers.”
- “This gives a measurable path from local experimentation to production-style operations.”

---

## Suggested close

- Current repo gives a working analytics and orchestration baseline.
- Next increments (from PRD) add direct AI provider demos and proxy services on top of this foundation.
