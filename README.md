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

## Foundry Local samples

Five standalone console samples are available under `samples\`:

- `01-foundrylocal-hello-world` — non-streaming single prompt/response with preflight checks.
- `02-foundrylocal-streaming` — streaming token-by-token output with prompt variants (`eli5`, `bullets`).
- `03-foundrylocal-scenarios` — practical scenarios (`summarize`, `sentiment`, `structured`) with deterministic prompts.
- `04-foundrylocal-native-chat-completions` — Microsoft Learn parity sample for **native SDK chat completions** (in-process `FoundryLocalManager` flow). Use this when you need direct SDK model lifecycle control (discover/register EPs, download/load/unload model), not OpenAI endpoint mode.
- `05-foundrylocal-audio-transcription` — Microsoft Learn parity sample for **native SDK audio transcription** (download/load whisper model, prefer CPU variant, stream transcript output from an audio file).

### Prerequisites and environment variables

- .NET SDK `10.0.301`
- Foundry Local service running and reachable (default endpoint: `http://127.0.0.1:5273/v1`)
- Optional environment variables:
  - `FOUNDRY_LOCAL_BASE_URL` (default: `http://127.0.0.1:5273/v1`)
  - `FOUNDRY_LOCAL_MODEL` (default: `qwen2.5-0.5b`)
  - `FOUNDRY_LOCAL_API_KEY` (default: `local-dev-key`)
  - `FOUNDRY_LOCAL_PROMPT_VARIANT` (streaming sample only; default: `eli5`)
  - `FOUNDRY_LOCAL_NATIVE_MODEL` (native sample alias override; falls back to `FOUNDRY_LOCAL_MODEL`)
  - `FOUNDRY_LOCAL_WHISPER_MODEL` / `FOUNDRY_LOCAL_AUDIO_MODEL` (audio transcription sample alias override; default: `whisper-tiny`)
  - `FOUNDRY_LOCAL_AUDIO_LANGUAGE` (audio transcription language hint; default: `en`)

### Commands

```powershell
# 01 - hello world
cd samples\01-foundrylocal-hello-world
dotnet restore
dotnet run

# 02 - streaming
cd ..\02-foundrylocal-streaming
dotnet restore
dotnet run
# optional variant
dotnet run -- bullets

# 03 - scenarios
cd ..\03-foundrylocal-scenarios
dotnet restore
dotnet run -- summarize
dotnet run -- sentiment
dotnet run -- structured
# interactive menu
dotnet run

# 04 - native chat completions (Microsoft Learn parity; native SDK in-process, not OpenAI endpoint mode)
cd ..\04-foundrylocal-native-chat-completions
dotnet restore
dotnet run

# 05 - native audio transcription (Microsoft Learn parity)
cd ..\05-foundrylocal-audio-transcription
dotnet restore
dotnet run
# optional custom file
dotnet run -- "C:\path\to\audio.mp3"
```

Detailed flow, expected output, and troubleshooting:
[docs\foundry-local-samples-runbook.md](docs/foundry-local-samples-runbook.md)

## Troubleshooting

- **SDK not found / wrong version:** run `dotnet --info` and ensure SDK `10.0.301` is installed.
- **HTTPS certificate warning:** run `dotnet dev-certs https --trust`.
- **Port already in use:** stop conflicting process or launch with another profile/port.
- **Dashboard is empty:** confirm API is running and reachable at the configured base URL.

## More docs

- Step-by-step runbook: `docs\demo-runbook.md`
- Demo narrative/story flow: `docs\demo-story-flow.md`
- Foundry Local samples runbook: [docs\foundry-local-samples-runbook.md](docs/foundry-local-samples-runbook.md)
- Product requirements: `docs\Local-AI-with-dotNET-PRD.md`
