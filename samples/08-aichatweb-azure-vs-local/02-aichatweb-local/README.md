# 02-aichatweb-local (Aspire + Foundry Local)

This is the local variant of the AI Chat Web template generated with `--aspire`. It uses `ElBruno.MarkItDotNet` in-process for document conversion, so there is no Docker dependency for MarkItDown.

Key local seams in `02-aichatweb-local.Web\Program.cs`:

- Chat: `IChatClient` via Foundry Local (`ElBruno.MAF.FoundryLocal.Adapter`)
- Embeddings: `IEmbeddingGenerator` via `ElBruno.LocalEmbeddings`

Everything else (UI, ingestion, vector-store usage) remains aligned with the template.

## Run

```powershell
dotnet run --project .\02-aichatweb-local.AppHost\02-aichatweb-local.AppHost.csproj
```

Then in the web app:

1. Use **Prepare model** (download icon) to download/load the configured model if needed.
2. Use **Refresh model status** (refresh icon) to re-check the local model cache/load state.
3. Optional model actions:
   - **Open model location** (folder icon) when a model is downloaded.
   - **Delete downloaded model** (trash icon) to clear local cache.

See [Model Status and Actions](./docs/model-status-and-actions.md) for the full lifecycle and troubleshooting.

## Notes

- AppHost sets `FoundryLocal__ModelAlias=phi-4-mini` for the Web project.
- Document-to-Markdown conversion stays in-process via `ElBruno.MarkItDotNet`.
- If you switch embedding providers, clear local ingestion/vector-store artifacts before rerunning so documents are re-embedded with the expected dimensions.
