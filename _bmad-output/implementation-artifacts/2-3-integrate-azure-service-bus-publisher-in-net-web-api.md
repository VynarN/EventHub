# Story 2.3: Integrate Azure Service Bus Publisher in .NET Web API

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to integrate an Azure Service Bus publisher into the .NET Web API,
So that events received by the API are reliably sent to Azure Service Bus for subsequent asynchronous processing.

## Acceptance Criteria

1. Given the POST `/api/events` endpoint is implemented (Story 2.2), When a new event is successfully received by the API, Then an `IEventPublisher` interface and `EventPublisher` concrete class are created in `EventHub.WebApi/Services/` for publishing messages to Azure Service Bus.
2. And the `EventPublisher` is configured to connect to the Azure Service Bus emulator (or a configured Service Bus instance).
3. And the `POST /api/events` endpoint utilizes the `IEventPublisher` to publish the newly created Event object as a message to a designated Azure Service Bus queue/topic.
4. And the event message payload adheres to the defined `Event.Created` event naming convention and includes the complete `Event` object.
5. And the API still responds with a 201 Created status code upon successful publishing.

## Tasks / Subtasks

- [x] Create `IEventPublisher.cs` interface in `EventHub.WebApi/Services/` (AC: #1)
- [x] Create `EventPublisher.cs` class in `EventHub.WebApi/Services/` implementing `IEventPublisher` (AC: #1)
- [x] Configure `EventPublisher` to connect to Azure Service Bus (emulator for local dev) (AC: #2)
- [x] Inject `IEventPublisher` into `EventsController` (AC: #3)
- [x] Modify `POST /api/events` endpoint to use `IEventPublisher` to publish Event object (AC: #3)
- [x] Ensure event message payload follows `Event.Created` naming convention and includes Event object (AC: #4)
- [x] Verify API still returns 201 Created status code (AC: #5)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Communication Between Services:** Azure Service Bus is used as the message broker. [Source: architecture.md#Decision: Communication Between Services]
-   **Event System Patterns:** Event Naming Convention: `Entity.Action` (e.g., `Event.Created`); Event Payload Structure Standards: Clearly defined DTOs. [Source: architecture.md#Event System Patterns (Azure Service Bus)]
-   **Local Development:** Use Azure Service Bus emulator. [Source: prd.md#Local Development Environment]
-   **File Organization:** Services in `EventHub.WebApi/Services/`. [Source: architecture.md#Complete Project Directory Structure]

### Source tree components to touch
-   `EventHub.WebApi/Services/IEventPublisher.cs` (new file)
-   `EventHub.WebApi/Services/EventPublisher.cs` (new file)
-   `EventHub.WebApi/Controllers/EventsController.cs` (modify existing)
-   `EventHub.WebApi/Program.cs` (for dependency injection setup)

### Testing standards summary
-   Unit tests for `EventPublisher` to verify message construction and sending.
-   Integration tests to ensure `POST /api/events` successfully publishes to the Service Bus emulator and maintains correct HTTP responses.
-   Manual testing by sending a POST request and verifying the message appears in the Service Bus emulator.

### Project Structure Notes
-   The `IEventPublisher` and `EventPublisher` files should be placed in `EventHub.WebApi/Services/`.

### References
-   [Source: epics.md#Story 2.3: Integrate Azure Service Bus Publisher in .NET Web API]
-   [Source: prd.md#Functional Requirements]
-   [Source: architecture.md#API & Communication Patterns]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Composer (dev-story workflow)

### Debug Log References

### Completion Notes List

- Implemented `IEventPublisher`, `EventPublisher`, and `NoOpEventPublisher`; `ServiceBusOptions` bound from `ServiceBus` configuration; `ServiceBusClient` registered when connection string is present.
- Messages use `Subject` = `Event.Created`, `ContentType` `application/json`, body = full `Event` JSON (camelCase + enum strings, aligned with API JSON options).
- `docker-compose` Web API service receives `ServiceBus__*` env vars for emulator; `appsettings.Development.json` documents localhost emulator connection string.
- Tests: `CapturingEventPublisher` + `ConfigureTestServices` for POST integration; `EventPublisherMessageTests` for message shape; `InternalsVisibleTo` for testing `CreateEventCreatedMessage`.

### File List

- EventHub.WebApi/Services/IEventPublisher.cs
- EventHub.WebApi/Services/EventPublisher.cs
- EventHub.WebApi/Services/NoOpEventPublisher.cs
- EventHub.WebApi/Options/ServiceBusOptions.cs
- EventHub.WebApi/Controllers/EventsController.cs
- EventHub.WebApi/Program.cs
- EventHub.WebApi/EventHub.WebApi.csproj
- EventHub.WebApi/appsettings.json
- EventHub.WebApi/appsettings.Development.json
- EventHub.WebApi/appsettings.Testing.json
- EventHub.WebApi.Tests/CapturingEventPublisher.cs
- EventHub.WebApi.Tests/TestWebApplicationFactory.cs
- EventHub.WebApi.Tests/EventsPostTests.cs
- EventHub.WebApi.Tests/EventPublisherMessageTests.cs
- EventHub.WebApi.Tests/EventHub.WebApi.Tests.csproj
- docker-compose.yml
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/implementation-artifacts/2-3-integrate-azure-service-bus-publisher-in-net-web-api.md

## Change Log

- 2026-03-19: Story 2.3 — Service Bus publisher, DI, POST integration, config, tests, sprint status.
