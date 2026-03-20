# Story 3.2: Store Events in CosmosDB from Azure Function App

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want the Azure Function App to store the received event data into Azure CosmosDB,
So that all events are durably persisted and available for retrieval.

## Acceptance Criteria

1. Given the Azure Function App successfully consumes messages from Azure Service Bus (Story 3.1), and the CosmosDB database and container are configured (Story 1.2), When the Azure Function App receives a valid `Event` message, Then the `EventConsumerFunction` (or a dedicated service within the function) connects to the `EventHub` CosmosDB database and `Events` container.
2. And the `Event` object is correctly stored as a document in the `Events` container.
3. And the `Id` field of the Event object is used as the partition key for the CosmosDB document.
4. And I can verify the event is successfully stored in the CosmosDB emulator by querying the database.
5. And any errors during CosmosDB storage are logged to Application Insights (NFR6, NFR7) and handled gracefully (e.g., dead-lettering the message if configured for Service Bus).

## Tasks / Subtasks

- [x] Introduce shared Cosmos event document shape and writer in `EventHub.Cosmos` (AC: #1, #2, #3)
- [x] Align `CosmosDbEventListReader` with the shared document type (AC: #2)
- [x] Persist deserialized events from `EventConsumerFunction` via DI-injected writer; log and propagate Cosmos failures (AC: #5)
- [x] Register `ICosmosEventWriter` when Cosmos is configured; no-op when connection string is absent (AC: #1)
- [x] Add automated tests (document mapping / writer behavior; optional skippable emulator write) (AC: #4)

## Dev Notes

### Relevant architecture patterns and constraints

- **Data access:** Denormalized event documents in `Events` container; partition key path `/Id` per `CosmosDbSettings`. [Source: architecture.md, Story 1.2]
- **Monitoring:** Application Insights for errors. [Source: architecture.md]
- **Service Bus:** Transient Cosmos failures should allow retry/DLQ; do not silently complete messages on write failure.

### Source tree components to touch

- `EventHub.Cosmos/` — document DTO, `ICosmosEventWriter`, registration
- `EventHub.WebApi/Services/CosmosDbEventListReader.cs` — use shared document type
- `EventHub.FunctionApp/EventConsumerFunction.cs`, `Program.cs` (DI already uses `AddEventHubCosmos`)
- `EventHub.FunctionApp.Tests/` — tests

### References

- [Source: epics.md#Story 3.2]
- [Source: 3-1-implement-azure-function-app-for-service-bus-consumption.md]

## Dev Agent Record

### Agent Model Used

Composer (dev-story workflow)

### Debug Log References

### Implementation Plan

- Shared `CosmosEventDocument` with `id` (document id) + `Id` partition property (`/Id`), camelCase query fields aligned with `EventCosmosQueryBuilder`.
- `CosmosEventWriter` upserts by partition key; logs errors and rethrows so Service Bus can retry / DLQ.
- `NoOpCosmosEventWriter` when `CosmosDb:ConnectionString` is unset (tests / local without emulator).
- Function consumer async: deserialize → upsert; Cosmos failure logged at Error with MessageId + EventId then rethrow.

### Completion Notes List

- Added `CosmosEventDocument`, `ICosmosEventWriter`, `CosmosEventWriter`, `NoOpCosmosEventWriter`; extended `AddEventHubCosmos` to register writer in both configured and unconfigured cases.
- `CosmosDbEventListReader` now deserializes query results as shared `CosmosEventDocument`.
- `EventConsumerFunction` upserts after successful deserialize; `EventDeserialization.SerializeEventType` matches Web API publisher enum casing.
- Tests: mapping unit tests in `EventHub.FunctionApp.Tests`; skippable `CosmosEventWriterIntegrationTests` when emulator is up (same pattern as Story 1.2 container test).

### File List

- EventHub.Cosmos/CosmosEventDocument.cs
- EventHub.Cosmos/ICosmosEventWriter.cs
- EventHub.Cosmos/CosmosEventWriter.cs
- EventHub.Cosmos/NoOpCosmosEventWriter.cs
- EventHub.Cosmos/ServiceCollectionExtensions.cs
- EventHub.WebApi/Services/CosmosDbEventListReader.cs
- EventHub.FunctionApp/EventConsumerFunction.cs
- EventHub.FunctionApp/EventDeserialization.cs
- EventHub.WebApi.Tests/CosmosEventWriterIntegrationTests.cs
- EventHub.FunctionApp.Tests/CosmosEventPersistenceMappingTests.cs
- _bmad-output/implementation-artifacts/3-2-store-events-in-cosmosdb-from-azure-function-app.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-03-19: Story 3.2 — persist Service Bus events to Cosmos via Function App; shared document DTO + writer; sprint status → review.

