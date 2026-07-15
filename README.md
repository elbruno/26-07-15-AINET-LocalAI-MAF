# Local AI with .NET (samples-first)

This repository is now organized as a **samples-first Foundry Local demo repo**. The main content lives under `samples\`, with sample `09` carrying the companion Aspire analytics app.

> Full session vision is in `docs\Local-AI-with-dotNET-PRD.md`.

## Prerequisites

- .NET SDK `10.0.301` (see `global.json`)
- Git
- (Recommended) Aspire workload/tools for the best AppHost experience:
  - `dotnet workload install aspire`

## Quick start

From repository root:

```powershell
dotnet restore dotnet-local-ai.slnx
dotnet build dotnet-local-ai.slnx
```

Then choose a sample under `samples\` and run its README instructions.

## Foundry Local samples

Nine standalone samples/projects are available under `samples\`:

- `01-foundrylocal-hello-world` — non-streaming single prompt/response with preflight checks.
- `02-foundrylocal-streaming` — streaming token-by-token output with prompt variants (`eli5`, `bullets`).
- `03-foundrylocal-scenarios` — practical scenarios (`summarize`, `sentiment`, `structured`) with deterministic prompts.
- `04-foundrylocal-native-chat-completions` — Microsoft Learn parity sample for **native SDK chat completions** (in-process `FoundryLocalManager` flow). Use this when you need direct SDK model lifecycle control (discover/register EPs, download/load/unload model), not OpenAI endpoint mode.
- `05-foundrylocal-audio-transcription` — Microsoft Learn parity sample for **native SDK audio transcription** (download/load whisper model, prefer CPU variant, stream transcript output from an audio file).
- `06-foundrylocal-native-auto-chat` — native SDK sample that resolves model alias, chooses best variant for machine capabilities (GPU/CPU), auto-downloads, asks a question with quality guard, and unloads.
- `07-foundrylocal-agent-tools` — local agent-style sample using `ElBruno.MAF.FoundryLocal.Adapter` + `Microsoft.Extensions.AI` tool invocation with tools defined in a separate file.
- `08-aichatweb-azure-vs-local` — side-by-side AI Chat Web template comparison: `01-aichatweb-azure` (cloud baseline) and `02-aichatweb-local` (Foundry Local chat + local embeddings).
- `09-analytics-aspire` — companion Aspire analytics app moved from `src\` into the samples folder.

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
  - `FOUNDRY_LOCAL_PROMPT` (native auto chat sample prompt override; default: `Why is the sky blue?`)
  - `FOUNDRY_LOCAL_CLEANUP_MODEL` (native auto chat cache cleanup toggle; `true` removes model cache after run)
  - `FOUNDRY_LOCAL_AGENT_PROMPT` (agent tools sample prompt override)
  - `FOUNDRY_LOCAL_AGENT_FALLBACK_MODEL` (agent tools sample fallback alias when no tool calls are detected)

### Commands

```powershell
# 01 - hello world
cd samples\01-foundrylocal-hello-world
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
dotnet restore
dotnet run

# 02 - streaming
cd ..\02-foundrylocal-streaming
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
$env:FOUNDRY_LOCAL_PROMPT_VARIANT="eli5"
dotnet restore
dotnet run
# optional variant
dotnet run -- bullets

# 03 - scenarios
cd ..\03-foundrylocal-scenarios
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
dotnet restore
dotnet run -- summarize
dotnet run -- sentiment
dotnet run -- structured
# interactive menu
dotnet run

# 04 - native chat completions (Microsoft Learn parity; native SDK in-process, not OpenAI endpoint mode)
cd ..\04-foundrylocal-native-chat-completions
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_NATIVE_MODEL="qwen2.5-0.5b"
dotnet restore
dotnet run

# 05 - native audio transcription (Microsoft Learn parity)
cd ..\05-foundrylocal-audio-transcription
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_WHISPER_MODEL="whisper-tiny"
$env:FOUNDRY_LOCAL_AUDIO_MODEL="whisper-tiny"
$env:FOUNDRY_LOCAL_MODEL="whisper-tiny"
$env:FOUNDRY_LOCAL_AUDIO_LANGUAGE="en"
dotnet restore
dotnet run
# optional custom file
dotnet run -- "C:\path\to\audio.mp3"

# 06 - native auto chat (SDK-first, no endpoint URL config)
cd ..\06-foundrylocal-native-auto-chat
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_MODEL="phi-3.5-mini"
$env:FOUNDRY_LOCAL_PROMPT="Why is the sky blue?"
dotnet restore
dotnet run
# end-of-run prompt defaults to Yes: "Delete downloaded model? [Y/n]"
# optional non-interactive override:
$env:FOUNDRY_LOCAL_CLEANUP_MODEL="true"
dotnet run

# 07 - local agent + tools (adapter + MEAI function invocation)
cd ..\07-foundrylocal-agent-tools
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_AGENT_FALLBACK_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_AGENT_PROMPT="I am in Pacific Standard Time. Bill is 42.50 with 18% tip. Use tools and return JSON."
$env:FOUNDRY_LOCAL_CLEANUP_MODEL="false"
dotnet restore
dotnet run
# end-of-run prompt defaults to Yes: "Delete downloaded model? [Y/n]"
# optional non-interactive override:
$env:FOUNDRY_LOCAL_CLEANUP_MODEL="true"
dotnet run

# 08 - AI Chat Web template (Azure vs Local, Aspire)
cd ..\08-aichatweb-azure-vs-local\01-aichatweb-azure
dotnet run --project .\01-aichatweb-azure.AppHost\01-aichatweb-azure.AppHost.csproj
# configure Azure OpenAI values as prompted by Aspire local provisioning

cd ..\02-aichatweb-local
foundry model run phi-4-mini
dotnet run --project .\02-aichatweb-local.AppHost\02-aichatweb-local.AppHost.csproj
```

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
