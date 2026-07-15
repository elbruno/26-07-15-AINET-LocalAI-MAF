# 06-foundrylocal-native-auto-chat

Foundry Local **native SDK** sample that does in-process chat with automatic runtime/model lifecycle steps.

## What it does

1. Initializes `FoundryLocalManager` (no REST endpoint URL required)
2. Discovers and registers execution providers
3. Resolves model alias from catalog
4. Downloads model if needed (cached on next run)
5. Loads model, streams chat response, and unloads model

## Configuration

Environment variables (all optional):

- `FOUNDRY_LOCAL_MODEL` (default: `qwen2.5-0.5b`)
- `FOUNDRY_LOCAL_PROMPT` (default: `Why is the sky blue?`)

## Run

```powershell
cd samples\06-foundrylocal-native-auto-chat
# optional overrides
$env:FOUNDRY_LOCAL_MODEL="qwen2.5-0.5b"
$env:FOUNDRY_LOCAL_PROMPT="Why is the sky blue?"
dotnet restore
dotnet run
```

## Expected output

Output includes:

- `Foundry Local native auto chat sample`
- `Available execution providers:`
- `Downloading/registering execution providers:`
- `Resolved model:`
- `Downloading model:`
- `Chat completion response:`
- `Model unloaded.`
