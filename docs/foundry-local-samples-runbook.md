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

## 5) Troubleshooting

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
