# Foundry Local samples runbook

Back to overview: [`README.md` Foundry Local samples section](../README.md#foundry-local-samples)

## 1) Setup

From repository root:

```powershell
dotnet --info
```

Verify Foundry Local is installed and running:

```powershell
foundry --help
foundry service status
```

If service is not running:

```powershell
foundry service start
```

Optional environment overrides:

```powershell
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
```

## 2) Run sample 01 (hello world, non-streaming)

```powershell
cd samples\01-foundrylocal-hello-world
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
dotnet restore
dotnet run
```

Expected output includes:

- `Foundry Local hello world (non-streaming)`
- `Endpoint: .../v1`
- `Model response:`

## 3) Run sample 02 (streaming)

```powershell
cd ..\02-foundrylocal-streaming
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
$env:FOUNDRY_LOCAL_PROMPT_VARIANT="eli5"
dotnet restore
dotnet run
```

Optional variant:

```powershell
dotnet run -- bullets
```

Expected output includes:

- `Foundry Local streaming sample`
- `Selected variant: ...`
- `Streaming response:`
- Incremental streamed text

## 4) Run sample 03 (practical scenarios)

```powershell
cd ..\03-foundrylocal-scenarios
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_API_KEY="local-dev-key"
dotnet restore
```

Run each scenario:

```powershell
dotnet run -- summarize
dotnet run -- sentiment
dotnet run -- structured
```

Interactive mode:

```powershell
dotnet run
```

Expected output includes:

- `Foundry Local practical scenarios`
- `Scenario: summarize|sentiment|structured`
- `Input:`
- `Output:`

## 5) Run sample 04 (native chat completions, Microsoft Learn parity)

Prerequisites for this sample:

- Windows target/package is already configured in this repo (`net8.0-windows10.0.18362` + `Microsoft.AI.Foundry.Local.WinML` in `samples\04-foundrylocal-native-chat-completions\FoundryLocal.NativeChatCompletions.csproj`).
- Foundry Local service installed on this Windows machine.

Optional model alias override:

```powershell
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_NATIVE_MODEL="qwen2.5-0.5b"
```

Run:

```powershell
cd ..\04-foundrylocal-native-chat-completions
dotnet restore
dotnet run
```

Expected output includes:

- `Foundry Local native chat completions sample`
- `Execution providers:`
- `Resolved model:`
- `Streaming chat completion:`
- `Model unloaded.`

Troubleshooting note:

- If restore/build fails on non-Windows OS, run sample `04` on Windows or switch to package `Microsoft.AI.Foundry.Local` for cross-platform use.

## 6) Run sample 05 (native audio transcription, Microsoft Learn parity)

Prerequisites for this sample:

- Windows target/package is already configured in this repo (`net8.0-windows10.0.18362` + `Microsoft.AI.Foundry.Local.WinML` in `samples\05-foundrylocal-audio-transcription\FoundryLocal.AudioTranscription.csproj`).
- Foundry Local service installed on this Windows machine.

Optional model alias override:

```powershell
$env:FOUNDRY_LOCAL_WHISPER_MODEL="whisper-tiny"
$env:FOUNDRY_LOCAL_AUDIO_MODEL="whisper-tiny"
$env:FOUNDRY_LOCAL_MODEL="whisper-tiny"
$env:FOUNDRY_LOCAL_AUDIO_LANGUAGE="en"
```

Run:

```powershell
cd ..\05-foundrylocal-audio-transcription
dotnet restore
dotnet run
# optional custom audio file path
dotnet run -- "C:\path\to\audio.mp3"
```

Expected output includes:

- `Foundry Local native audio transcription sample`
- `Execution providers:`
- `Resolved model:`
- `Selected CPU variant:` (when available)
- `Transcribing audio with streaming output:`
- Streamed transcription text
- `Model unloaded.`

Input file behavior:

- If an argument is provided, that path is used.
- If no argument is provided, sample defaults to `samples\05-foundrylocal-audio-transcription\Recording.mp3`.
- If the file is missing, the sample prints an actionable message and exits.

## 7) Run sample 06 (native auto chat, SDK-first)

This sample does not use an OpenAI-compatible endpoint URL. It uses the native SDK flow to discover/register EPs, resolve model alias, auto-download if needed, load, run chat with quality guard, and unload.

```powershell
cd ..\06-foundrylocal-native-auto-chat
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_MODEL="phi-3.5-mini"
$env:FOUNDRY_LOCAL_PROMPT="Why is the sky blue?"
dotnet restore
dotnet run
```

At the end, the sample asks:

```text
Delete downloaded model? [Y/n]
```

Default is **Yes**.

For non-interactive runs, force cleanup behavior:

```powershell
$env:FOUNDRY_LOCAL_CLEANUP_MODEL="true"
dotnet run
```

Expected output includes:

- `Foundry Local native auto chat sample`
- `Step 1/6 ... Step 6/6` (live-demo friendly progress)
- `Available execution providers:`
- `Downloading/registering execution providers:`
- `Resolved model alias:`
- `Selected variant: ... (GPU|CPU)` (based on registered machine capabilities)
- `Question: Why is the sky blue?` (or your prompt override)
- `Prompt: Why is the sky blue?` (shown in Step 6)
- `Answer:`
- `Primary response looked malformed or off-topic. Retrying once...` (only if fallback kicks in)
- `Model unloaded.`
- `Delete downloaded model? [Y/n]`
- `Model cache removed.` (when answer is yes, or override is `true`)

## 8) Run sample 07 (local agent + tools)

This sample uses `ElBruno.MAF.FoundryLocal.Adapter` with `Microsoft.Extensions.AI` function invocation middleware to run an agent-like local turn with tools.

```powershell
cd ..\07-foundrylocal-agent-tools
# optional overrides (defaults shown)
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_AGENT_FALLBACK_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_AGENT_PROMPT="I am in Pacific Standard Time. Bill is 42.50 with 18% tip. Use tools and return JSON."
$env:FOUNDRY_LOCAL_CLEANUP_MODEL="false"
dotnet restore
dotnet run
```

Expected output includes:

- `Foundry Local agent + tools sample`
- `Step 1/6 ... Step 6/6`
- `Model cache: already available locally.` or `Model cache: not present. It will be downloaded.`
- `Registered tools: get_time_in_timezone, calculate_tip, get_demo_fact`
- `[tool:...]` console logs showing each tool invocation and result
- `No tool calls were detected ... Try running again with fallback model ...` (only when selected model skips tool use)
- `Agent response:`
- `Delete downloaded model? [Y/n]`

## 9) Run sample 08 (AI Chat Web template: Azure vs Local, Aspire)

This scenario has two Aspire outputs under `samples\08-aichatweb-azure-vs-local\`.

Run Azure baseline:

```powershell
cd ..\08-aichatweb-azure-vs-local\01-aichatweb-azure
dotnet run --project .\01-aichatweb-azure.AppHost\01-aichatweb-azure.AppHost.csproj
```

Run local baseline:

```powershell
foundry model run phi-4-mini
cd ..\02-aichatweb-local
dotnet run --project .\02-aichatweb-local.AppHost\02-aichatweb-local.AppHost.csproj
```

Expected behavior:

- AppHost + Web projects start for each variant.
- Azure variant uses Azure OpenAI provisioning from AppHost.
- Local variant uses Foundry Local chat + `ElBruno.LocalEmbeddings` with the same UI and ingestion flow.
- Both variants convert docs in-process with `ElBruno.MarkItDotNet`, so no MarkItDown Docker container is needed.
- Both use the local Sqlite vector store for retrieved-citation answers.

## 10) Run sample 09 (companion Aspire analytics app)

This sample now lives under `samples\09-analytics-aspire\`.

```powershell
aspire start --apphost samples\09-analytics-aspire\AppHost\AppHost.csproj
```

Direct runs:

```powershell
dotnet run --project samples\09-analytics-aspire\analytics\Analytics.Api\Analytics.Api.csproj
dotnet run --project samples\09-analytics-aspire\analytics\Analytics.Web\Analytics.Web.csproj
```

Expected behavior:

- Aspire starts the analytics API and web dashboard from the moved sample folder.
- The dashboard can be opened from the Aspire UI or directly at the local HTTPS endpoint.

## 11) Troubleshooting

### Service offline / unreachable

Symptoms:

- `Preflight check failed.`
- `Could not reach Foundry Local service at '.../v1'`

Actions:

```powershell
foundry service start
foundry service status
$env:FOUNDRY_LOCAL_BASE_URL="http://127.0.0.1:5273/v1"
```

### Model missing

Symptom:

- `Configured model '...' was not found. Using '...' instead.`

Actions:

```powershell
$env:FOUNDRY_LOCAL_MODEL="<an-installed-model-id>"
```

If needed, inspect available models from your local endpoint:

```powershell
curl http://127.0.0.1:5273/v1/models
```
