# Story 5.3: Verify System Scalability and Reliability

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to verify that the system architecture and configuration support the defined scalability and reliability non-functional requirements,
So that the EventHub can handle expected loads and operate dependably.

## Acceptance Criteria

1. Given all application components are implemented (Epics 1–4) and monitoring exists (Story 5.2), when validating in staging or via local simulation, then scalability-related architecture choices (App Service autoscale, Functions Consumption, Service Bus Premium option, Cosmos autoscale, SPA CDN) are documented with an Azure configuration checklist.
2. And the repository provides repeatable **k6** load scripts for **POST /api/events** (ingestion) and **GET /api/events** (read path) with **smoke** defaults for local/docker-compose and **strict** (`NFR_STRICT=true`) thresholds oriented to NFR3 (p95 API latency).
3. And at-least-once delivery (NFR2) is explained in documentation with references to Service Bus trigger completion semantics and idempotent **Cosmos upsert** by event id.
4. And Application Insights is referenced as the place to confirm request/dependency latency and errors against NFR3/NFR6/NFR7, with sample Kusto for p95.
5. And a unit test locks the **PagedEventsResponse** JSON shape (`items`, pagination fields) used by load-test clients.

## Tasks / Subtasks

- [x] Add NFR verification document with architecture table, NFR mapping, at-least-once + upsert, App Insights notes (AC: #1, #3, #4)
- [x] Add `load-tests/k6` scripts (ingest smoke/nfr, list smoke/nfr) and `load-tests/README.md` with run instructions (AC: #2)
- [x] Add Web API JSON test for paged list contract used by k6 (AC: #5)
- [x] Run `dotnet test` on `EventHub.WebApi.Tests` and `EventHub.FunctionApp.Tests`; record k6 execution note if Docker unavailable (AC: #2)

## Dev Notes

### Relevant architecture patterns and constraints

- **Scaling Strategy:** SPA (SWA/CDN), API (App Service autoscale), Functions (Consumption), Service Bus (Premium for high throughput), Cosmos (autoscale, partition key `Id`). [Source: architecture.md#Decision: Scaling Strategy]
- **Async events:** Idempotency for redelivered Service Bus messages. [Source: architecture.md#Communication Patterns]
- **Documentation placement:** `docs/` at repo root. [Source: architecture.md#File Structure Patterns]

### Source tree components to touch

- `docs/nfr-scalability-reliability-verification.md` (new)
- `load-tests/README.md`, `load-tests/k6/*.js` (new)
- `EventHub.WebApi.Tests/EventModelJsonTests.cs` (extend)

### Testing standards summary

- Automated: `dotnet test` for JSON contract; k6 smoke/strict against a running API (host or Docker).
- Full NFR1 numbers (1,000 events/s, 100k events in 5s) require appropriately provisioned Azure staging; local emulators are not expected to satisfy those figures.

### References

- [Source: epics.md#Story 5.3: Verify System Scalability and Reliability]
- [Source: epics.md#NFR1, NFR2, NFR3, NFR4]
- [Source: architecture.md#Decision: Scaling Strategy]

## Dev Agent Record

### Agent Model Used

(Composer)

### Implementation Plan

- Capture scalability/reliability verification in `docs/nfr-scalability-reliability-verification.md` aligned with architecture decisions and existing code paths (`EventConsumerFunction`, `CosmosEventWriter`).
- Ship k6 scripts with smoke vs `NFR_STRICT` modes; document BASE_URL and Docker invocation in `load-tests/README.md`.
- Add `PagedEventsResponse` serialization test so list load tests stay aligned with camelCase JSON.

### Debug Log References

### Completion Notes List

- NFR verification doc links Azure checklist, NFR table, at-least-once (peek-lock + abandon on failure) and upsert idempotency, App Insights Kusto sample.
- k6: `ingest_smoke.js`, `ingest_nfr.js`, `list_events_smoke.js`, `list_events_nfr.js`; README explains smoke vs strict and staging tuning for heavy read throughput.
- `EventModelJsonTests.PagedEventsResponse_serializes_items_array_for_clients_and_load_tests` added.
- **k6 execution:** Docker engine returned an error on the dev machine (`docker run grafana/k6`); scripts and README are validated structurally; run k6 when Docker or a local k6 binary is available with stack up.

### File List

- `docs/nfr-scalability-reliability-verification.md`
- `load-tests/README.md`
- `load-tests/k6/ingest_smoke.js`
- `load-tests/k6/ingest_nfr.js`
- `load-tests/k6/list_events_smoke.js`
- `load-tests/k6/list_events_nfr.js`
- `EventHub.WebApi.Tests/EventModelJsonTests.cs`
- `_bmad-output/implementation-artifacts/5-3-verify-system-scalability-and-reliability.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`

## Change Log

- 2026-03-20: Story 5-3 — NFR verification doc, k6 load tests, paged JSON contract test; sprint status → review.

## Senior Developer Review (AI)

_(None)_
