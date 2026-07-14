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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
