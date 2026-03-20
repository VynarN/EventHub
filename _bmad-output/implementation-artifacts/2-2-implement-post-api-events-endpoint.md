# Story 2.2: Implement POST /api/events Endpoint

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to create a POST endpoint in the .NET Web API to receive new event data,
So that the Angular SPA can submit new events to the backend for processing.

## Acceptance Criteria

1. Given the Event model and DTOs are defined (Story 2.1), When a valid POST request is sent to `/api/events` with an `EventCreationDto` payload, Then the .NET Web API (`EventHub.WebApi/Controllers/EventsController.cs`) exposes a `POST /api/events` endpoint.
2. And the endpoint accepts an `EventCreationDto` as input.
3. And the endpoint automatically generates a unique `GUID` for the `Id` field of the Event.
4. And the `CreatedAt` field is automatically set to the current `DateTime`.
5. And the API responds with a 201 Created status code and the newly created Event object upon successful creation.
6. And invalid payloads result in appropriate RFC 9457 compliant error responses (e.g., 400 Bad Request).
7. And basic validation is performed on incoming `EventCreationDto` fields (e.g., `UserId` and `Type` are not empty).

## Tasks / Subtasks

- [x] Create `EventsController.cs` in `EventHub.WebApi/Controllers/` (AC: #1)
- [x] Implement `POST /api/events` action method (AC: #1)
- [x] Configure action to accept `EventCreationDto` from body (AC: #2)
- [x] Generate `GUID` for `Id` field (AC: #3)
- [x] Set `CreatedAt` to current `DateTime` (AC: #4)
- [x] Return 201 Created status code with created Event object (AC: #5)
- [x] Implement RFC 9457 compliant error handling for invalid payloads (AC: #6)
- [x] Add basic validation for `UserId` and `Type` fields in `EventCreationDto` (AC: #7)

## Dev Notes

### Relevant architecture patterns and constraints
-   **API Design Pattern:** REST. [Source: architecture.md#Decision: API Design Pattern]
-   **Error Handling Standard:** RFC 9457 (Problem Details for HTTP APIs) for error responses. [Source: architecture.md#Decision: Error Handling Standard]
-   **API Naming Conventions:** `kebab-case` for resource paths (e.g., `/api/events`), `camelCase` for query parameters. [Source: architecture.md#API Naming Conventions (.NET Web API)]
-   **Data Exchange Formats:** `camelCase` for JSON properties, `ISO 8601` for dates. [Source: architecture.md#Data Exchange Formats]
-   **Code Naming Conventions:** `PascalCase` for C# class/method names, `camelCase` for local variables/public properties. [Source: architecture.md#Code Naming Conventions (Angular SPA, .NET Web API, Azure Function App)]
-   **File Organization:** Controllers in `EventHub.WebApi/Controllers/EventsController.cs`. [Source: architecture.md#Complete Project Directory Structure]

### Source tree components to touch
-   `EventHub.WebApi/Controllers/EventsController.cs` (new file, or modify if exists)
-   `EventHub.WebApi/Models/Event.cs` (use existing)
-   `EventHub.WebApi/Models/EventCreationDto.cs` (use existing)

### Testing standards summary
-   Unit tests for controller methods, especially for ID and CreatedAt generation, and validation logic.
-   Integration tests for the `/api/events` endpoint to verify correct request handling, response codes, and error formats.
-   Use tools like Postman or Insomnia to manually test the API endpoint.

### Project Structure Notes
-   The `EventsController.cs` should be placed in `EventHub.WebApi/Controllers/`.

### References
-   [Source: epics.md#Story 2.2: Implement POST /api/events Endpoint]
-   [Source: prd.md#Functional Requirements]
-   [Source: architecture.md#API & Communication Patterns]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Flash

### Debug Log References

### Implementation Plan

- Registered MVC controllers with camelCase JSON and string enums aligned with minimal API JSON options.
- `EventCreationDto.Type` is `EventType?` with `[Required]` so a missing `type` property does not silently default to `PageView`.
- `[ApiController]` model validation returns `application/problem+json` (`ValidationProblemDetails`) for invalid payloads (RFC 9457 family).

### Completion Notes List

- Implemented `POST /api/events` on `EventsController`: generates `Id`, sets `CreatedAt` to UTC now, returns `201 Created` with `Location` and body.
- Added data annotations on `EventCreationDto` (`UserId`, `Type`).
- Added integration tests: happy path, empty `userId`, missing `type`, invalid enum string.

Ultimate context engine analysis completed - comprehensive developer guide created

### File List

- EventHub.WebApi/Controllers/EventsController.cs
- EventHub.WebApi/Models/EventCreationDto.cs
- EventHub.WebApi/Program.cs
- EventHub.WebApi.Tests/EventsPostTests.cs
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/implementation-artifacts/2-2-implement-post-api-events-endpoint.md

### Change Log

- 2026-03-19: Story 2.2 — POST `/api/events`, validation, integration tests; sprint status → review.