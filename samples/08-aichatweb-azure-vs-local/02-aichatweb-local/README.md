# 02-aichatweb-local (Aspire + Foundry Local)

This is the local variant of the AI Chat Web template generated with `--aspire`.

Key local seams in `02-aichatweb-local.Web\Program.cs`:

- Chat: `IChatClient` via Foundry Local (`ElBruno.MAF.FoundryLocal.Adapter`)
- Embeddings: `IEmbeddingGenerator` via `ElBruno.LocalEmbeddings`

Everything else (UI, ingestion, vector-store usage) remains aligned with the template.

## Run

```powershell
foundry model run phi-4-mini
dotnet run --project .\02-aichatweb-local.AppHost\02-aichatweb-local.AppHost.csproj
```

## Notes

- AppHost sets `FoundryLocal__ModelAlias=phi-4-mini` for the Web project.
- If you switch embedding providers, clear local ingestion/vector-store artifacts before rerunning so documents are re-embedded with the expected dimensions.
