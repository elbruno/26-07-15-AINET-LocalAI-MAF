# Foundry Local + Sample 11 (5-minute video/demo script)

## Goal
Deliver a fast, engineering-trustworthy demo of `samples\11-foundrylocal-live-transcription`, grounded in actual code behavior.

---

## Timestamped run-of-show

| Time | Segment | Presenter lines | Demo choreography |
|---|---|---|---|
| 00:00–00:30 | Hook | “Local AI goes beyond chat—speech is first-class too. In 5 minutes, I’ll run real-time local transcription with Foundry Local.” | Show repo root, then open `samples\11-foundrylocal-live-transcription\Program.cs`. |
| 00:30–01:00 | Model-selection framing | “This sample is alias-driven: users pick the model alias; Foundry Local handles acquisition and runtime setup.” | Point to alias selection in `PromptModelChoice(...)` and env vars (`FOUNDRY_LOCAL_SPEECH_MODEL`, `FOUNDRY_LOCAL_SPEECH_LANGUAGE`). |
| 01:00–01:45 | Lifecycle in code | “The runtime flow is explicit: discover execution providers, resolve catalog model, download, load, stream, then cleanup.” | Scroll to these calls in order: `DiscoverEps()` + `DownloadAndRegisterEpsAsync(...)`, `GetCatalogAsync()` + `GetModelAsync(modelAlias)`, `DownloadAsync(...)`, `LoadAsync()`, `CreateLiveTranscriptionSession()`, `GetStream(...)`, `UnloadAsync()`, `RemoveFromCacheAsync()`. |
| 01:45–03:30 | Live run | “Now let’s run it live and speak into the mic.” | In terminal: `cd samples\11-foundrylocal-live-transcription` then `dotnet run`. Select model (default English). Speak 1–2 lines. Call out cyan interim text and `[FINAL]` finalized lines. |
| 03:30–04:20 | NVIDIA/Nemotron alias precision | “Catalog aliases available here include NVIDIA/Nemotron streaming ASR options: `nemotron-speech-streaming-en-0.6b`, `nemotron-speech-streaming-es-0.6b`, and `nemotron-3.5-asr-streaming-0.6b`.” | Briefly show `README.md` alias table (or error/help text in `Program.cs`) and return to terminal output. |
| 04:20–04:45 | First run vs cached run | “First run downloads execution providers and model artifacts. Cached runs skip that heavy path and start faster from local cache.” | If available, show download lines from current run output. Mention cleanup prompt controls whether next run is cached. |
| 04:45–05:00 | Close | “That’s full local streaming speech: alias selection, Foundry-managed runtime, real-time interim/final transcription, and clean model lifecycle.” | Stop run with Enter. Show unload + optional delete prompt. |

---

## Exact code-reference checklist (sample 11)

- Discover/runtime providers:
  - `var executionProviders = manager.DiscoverEps();`
  - `await manager.DownloadAndRegisterEpsAsync(...);`
- Catalog + alias resolution:
  - `var catalog = await manager.GetCatalogAsync();`
  - `var model = await catalog.GetModelAsync(modelAlias);`
- First-run acquisition:
  - `await model.DownloadAsync(...);`
- Model load:
  - `await model.LoadAsync();`
- Live streaming session:
  - `var session = audioClient.CreateLiveTranscriptionSession();`
  - `await session.StartAsync(...);`
  - `await foreach (var result in session.GetStream(...))`
  - Interim path: `if (!result.IsFinal) ...`
  - Final path: `if (result.IsFinal) ... [FINAL]`
- Cleanup:
  - `await loadedModel.UnloadAsync();`
  - Optional cache removal: `await loadedModel.RemoveFromCacheAsync();`

---

## Demo commands

```powershell
cd samples\11-foundrylocal-live-transcription
dotnet run
```

Optional deterministic model override:

```powershell
$env:FOUNDRY_LOCAL_SPEECH_MODEL="nemotron-speech-streaming-en-0.6b"
$env:FOUNDRY_LOCAL_SPEECH_LANGUAGE="en"
dotnet run
```

---

## Fallback line (if live audio/demo stumbles)

“Even if mic capture is noisy in this environment, the code path is the same: alias selection → Foundry catalog resolution → first-run download/load → live session interim/final stream → unload/optional cache cleanup.”
