# Story 3.1: Implement Azure Function App for Service Bus Consumption

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to create an Azure Function App that consumes messages from Azure Service Bus,
So that event data published by the .NET Web API can be asynchronously processed.

## Acceptance Criteria

1. Given the Azure Function App project is initialized (Story 1.3), and events are being published to Azure Service Bus (Story 2.3), When a message containing an `Event` object arrives on the designated Azure Service Bus queue/topic, Then an `EventConsumerFunction.cs` (or similar) is created in `EventHub.FunctionApp/` as an Azure Function.
2. And the function is configured with an Azure Service Bus trigger that listens to the designated queue/topic.
3. And the function successfully deserializes the incoming message into an `Event` object.
4. And the function logs the received event details to Application Insights for monitoring (NFR6, NFR7).
5. And the function handles potential deserialization errors gracefully, logging failures but not blocking the Service Bus queue.

## Tasks / Subtasks

- [x] Create `EventConsumerFunction.cs` in `EventHub.FunctionApp/` (AC: #1)
- [x] Configure Azure Service Bus trigger for the function (AC: #2)
- [x] Implement logic to deserialize incoming Service Bus message into `Event` object (AC: #3)
- [x] Integrate Application Insights for logging (AC: #4)
- [x] Implement error handling for deserialization failures (AC: #5)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Communication Between Services:** Azure Service Bus is the message broker. [Source: architecture.md#Decision: Communication Between Services]
-   **Event System Patterns:** `Event.Created` naming convention, clear DTOs for event payloads, idempotency for async handling. [Source: architecture.md#Event System Patterns (Azure Service Bus)]
-   **Monitoring and Logging:** Azure Application Insights for logging exceptions and critical errors. [Source: architecture.md#Decision: Monitoring and Logging]
-   **Code Naming Conventions:** `PascalCase` for C# class/method names. [Source: architecture.md#Code Naming Conventions (Angular SPA, .NET Web API, Azure Function App)]
-   **File Organization:** Azure Function App functions in `EventHub.FunctionApp/`. [Source: architecture.md#Complete Project Directory Structure]

### Source tree components to touch
-   `EventHub.FunctionApp/EventConsumerFunction.cs` (new file)
-   `EventHub.FunctionApp/local.settings.json` (for Service Bus connection string and queue/topic name)
-   `EventHub.FunctionApp/host.json` (for Application Insights configuration)
-   `EventHub.WebApi/Models/Event.cs` (reference for `Event` object structure)

### Testing standards summary
-   Unit tests for message deserialization and error handling logic within the function.
-   Integration tests: Publish a message to the Service Bus emulator from the .NET Web API, then verify the Azure Function App processes it and logs correctly.
-   Monitor Application Insights for logs and metrics.

### Project Structure Notes
-   The `EventConsumerFunction.cs` should be placed in `EventHub.FunctionApp/`.

### References
-   [Source: epics.md#Story 3.1: Implement Azure Function App for Service Bus Consumption]
-   [Source: prd.md#Azure Function App]
-   [Source: architecture.md#API & Communication Patterns]
-   [Source: architecture.md#Infrastructure & Deployment]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Composer (dev-story workflow)

### Debug Log References

### Implementation Plan

- Service Bus trigger on `queue.1` + `ServiceBusConnection` (local + docker-compose parity with Web API).
- Deserialize with options matching `EventPublisher` (camelCase, string enums); isolate parsing in `EventDeserialization` for unit tests.
- On parse failure: warn and return (complete message) per AC #5.
- Log successful receives at Information for Application Insights.

### Completion Notes List

- Added `EventConsumerFunction` with `ServiceBusTrigger` on `queue.1` using `ServiceBusConnection`, matching Web API publisher and docker-compose.
- Introduced `Event` / `EventType` in `EventHub.FunctionApp.Models` aligned with `EventHub.WebApi.Models` JSON shape (camelCase + string enums) via shared `EventDeserialization` options.
- Deserialization failures are logged with `LogWarning` and the function returns without throwing so the trigger completes the message (no retry/DLQ churn from bad payloads).
- Application Insights: existing `Program.cs` worker telemetry + `host.json` logging settings; structured `LogInformation` for successful receives.
- Replaced placeholder `ServiceBusQueueListenerFunction` to avoid duplicate consumers on the same queue.
- Unit tests: `EventHub.FunctionApp.Tests` covering happy path, invalid JSON, and invalid enum.

### File List

- EventHub.FunctionApp/EventConsumerFunction.cs (new; replaces removed `ServiceBusQueueListenerFunction.cs`)
- EventHub.FunctionApp/EventDeserialization.cs
- EventHub.FunctionApp/Models/Event.cs
- EventHub.FunctionApp/Models/EventType.cs
- EventHub.FunctionApp.Tests/EventHub.FunctionApp.Tests.csproj
- EventHub.FunctionApp.Tests/EventDeserializationTests.cs
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/implementation-artifacts/3-1-implement-azure-function-app-for-service-bus-consumption.md

### Change Log

- 2026-03-19: Story 3.1 — Service Bus consumer function, deserialization with graceful error handling, Function App unit tests; sprint status → review.