# Squad Decisions

## Active Decisions

### 2026-07-14: Team cast based on Local AI .NET PRD
- Universe selected: **Alien**
- Active delivery roster: Ripley (Lead), Bishop (Backend), Hicks (Frontend), Apone (Platform), Vasquez (Tester)
- Fixed support roles retained: Scribe, Ralph, Rai

### 2026-07-14: Batch 1 execution order and baseline approved
- Ripley established the first executable batch with an explicit todo dependency chain.
- Apone scaffolded the solution/project layout and baseline platform wiring for team implementation.
- Bishop delivered shared contracts plus analytics API ingest/query/summary baseline as the backend starting point.
- Hicks delivered dashboard shell baseline with API client integration, filters, KPI cards, and empty/error states.
- Vasquez established shared/API tests and CI workflow as the quality gate for Batch 1.

### 2026-07-14: Analytics query ordering must be deterministic
- Minor API fix from Vasquez set stable SQLite ordering as a required behavior for analytics query outputs.

### 2026-07-14: Demo documentation baseline and narrative are now explicit
- Ripley published `README.md`, `docs/demo-runbook.md`, and `docs/demo-story-flow.md`.
- These documents define the canonical demo startup flow and presentation sequence for current delivery.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

### 2026-07-14: Foundry Local sample suite baseline accepted
- Apone scaffolded Foundry Local samples structure and solution wiring for runnable sample composition.
- Bishop implemented `hello-world`, `streaming`, and `scenarios` Foundry Local samples.
- Apone and Bishop added a shared sample support helper to reduce duplication and keep sample behavior consistent.
- Ripley updated README coverage and authored a Foundry Local samples runbook for execution guidance.


### 2026-07-15: Sample 08-02 invocation and model diagnostics flow aligned
- Bishop fixed sample `08-02` tool invocation by aligning tool names with function invocation configuration.
- Hicks added model alias + model status UI using the Foundry lifecycle diagnostics service.
- Validation build passed for the `08-02` solution, confirming the combined changes are healthy.
