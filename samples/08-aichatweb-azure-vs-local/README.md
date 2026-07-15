# 08-aichatweb-azure-vs-local (Aspire)

This scenario contains two **Aspire-enabled** AI Chat Web template outputs:

- `01-aichatweb-azure` — Azure OpenAI baseline (`--provider azureopenai --vector-store local --aspire`)
- `02-aichatweb-local` — local baseline (`--provider ollama --vector-store local --aspire`) adapted to:
  - Foundry Local for chat (`IChatClient`)
  - `ElBruno.LocalEmbeddings` for embeddings (`IEmbeddingGenerator`)

## Run 01 (Azure + Aspire)

```powershell
cd samples\08-aichatweb-azure-vs-local\01-aichatweb-azure
dotnet run --project .\01-aichatweb-azure.AppHost\01-aichatweb-azure.AppHost.csproj
```

Configure Azure OpenAI values via user-secrets as prompted by Aspire local provisioning.

## Run 02 (Foundry Local + Aspire)

```powershell
foundry model run phi-4-mini
cd samples\08-aichatweb-azure-vs-local\02-aichatweb-local
dotnet run --project .\02-aichatweb-local.AppHost\02-aichatweb-local.AppHost.csproj
```

## Notes

- The local sample keeps the template UI, ingestion pipeline, and local vector store flow.
- If you switch embedding providers, clear any prior local ingestion/vector-store artifacts to force re-embedding with the correct dimensions.
