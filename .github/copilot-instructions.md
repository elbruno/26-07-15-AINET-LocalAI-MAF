# Copilot instructions for this repository

- Treat this repo as **samples-first**; keep new work under `samples\` unless explicitly asked otherwise.
- Target **.NET 10** for new AI sample projects unless a sample explicitly requires another target.
- Keep provider wiring behind `IChatClient` and `IEmbeddingGenerator` seams.
- For AI Chat Web template variants, avoid changing UI, ingestion flow, or vector store plumbing unless explicitly requested.
- Use `async`/`await`; do not add blocking `.Wait()`/`.Result()` calls in request paths.
- Never commit secrets. Use environment variables or user-secrets for local configuration.
- When adding or moving samples, update:
  - `README.md` sample catalog and commands
  - `docs/foundry-local-samples-runbook.md`
  - `dotnet-local-ai.slnx`
