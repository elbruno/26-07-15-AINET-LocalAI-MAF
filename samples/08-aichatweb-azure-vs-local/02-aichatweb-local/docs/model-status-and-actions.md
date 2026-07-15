# 02-aichatweb-local: Model status and actions

This page explains how model readiness is computed in sample **08-02**, what each header action does, and how to troubleshoot model state.

## Status lifecycle

The app uses `FoundryLocalModelStatusService` to combine:

- lifecycle diagnostics (`FoundryLocalModelLifecycleService.GetDiagnosticsSnapshot()`), and
- direct model checks (`IsCachedAsync`, `IsLoadedAsync`).

The header status shows one of these states:

- **Loaded** / **Loaded (downloaded this session)**  
  Model is loaded in memory and ready for chat.
- **Downloaded (not loaded)**  
  Model files exist locally but are not loaded yet.
- **Not downloaded**  
  Model cache is missing locally.
- **Unavailable (model not in catalog)**  
  The configured alias was not found in local Foundry catalog.
- **Status unavailable**  
  Unexpected runtime error while resolving status.

## Header actions

- **Prepare model** (download icon)  
  Ensures the model is available end-to-end:
  1. downloads model if missing,
  2. loads model into memory,
  3. refreshes status.
- **Refresh model status** (refresh icon)  
  Performs a fresh runtime probe of cache + load state.
- **Open model location** (folder icon)  
  Opens model cache folder in Explorer. Disabled when model is not downloaded.
- **Delete downloaded model** (trash icon)  
  Unloads and removes cached model files. Disabled when model is not downloaded.

## Recommended user flow

Use this sequence for the most reliable first-run experience:

1. Open the app and check model status text.
2. Click **Prepare model** once.
3. Wait until status changes to **Loaded**.
4. Ask one of the quick sample questions.
5. If needed, click **Refresh model status**.
6. Use **Open model location** only for inspection, and **Delete downloaded model** only when you want to force a clean re-download.

## Why this fixed the prior issue

Previous status refresh used only diagnostics snapshot values, which can stay stale when no model operation runs.  
Now refresh actively queries model cache/load state, and **Prepare model** performs explicit download + load.

Additionally, status and model actions now ensure `FoundryLocalManager` is initialized before catalog/model calls.  
This removes the startup error: `FoundryLocalManager has not been created. Call CreateAsync first.`

## Troubleshooting

If status is **Unavailable (model not in catalog)**:

1. Check configured alias in `02-aichatweb-local.Web\appsettings.json` (`FoundryLocal:ModelAlias`).
2. Ensure Foundry Local is installed and available on the machine.
3. Retry **Prepare model** after correcting alias/runtime.

If status is **Not downloaded** and Prepare fails:

1. Check the message appended in chat (error detail).
2. Verify local disk access and network availability for model download.
3. Click **Refresh model status** after resolving the issue.

If status is **Status unavailable**:

1. Click **Refresh model status** once (manager initialization is retried automatically).
2. Click **Prepare model**.
3. If still failing, restart the app host and retry.
