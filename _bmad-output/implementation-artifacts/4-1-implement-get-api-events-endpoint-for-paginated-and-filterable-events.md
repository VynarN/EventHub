# Story 4.1: Implement GET /api/events Endpoint for Paginated and Filterable Events

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to implement a GET endpoint in the .NET Web API to retrieve a paginated and filterable list of events from CosmosDB,
So that the Angular SPA can display events dynamically based on user selection.

## Acceptance Criteria

1. Given events are stored in CosmosDB (Story 3.2), When a GET request is sent to `/api/events`, Then the .NET Web API (`EventHub.WebApi/Controllers/EventsController.cs`) exposes a `GET /api/events` endpoint.
2. And the endpoint supports pagination parameters (`pageNumber`, `pageSize`) and returns a paginated list of `Event` objects.
3. And the endpoint supports filtering by `Type` and `UserId` parameters.
4. And the endpoint returns a 200 OK status code with a list of `Event` objects and pagination metadata.
5. And if no events are found, an empty list is returned with a 200 OK status.
6. And all query parameter names adhere to the `camelCase` convention as per the Architecture document.

## Tasks / Subtasks

- [x] Modify `EventsController.cs` to add `GET /api/events` action method (AC: #1)
- [x] Implement logic for paginated retrieval of events from CosmosDB (AC: #2)
- [x] Implement logic for filtering events by `Type` and `UserId` (AC: #3)
- [x] Return 200 OK status code with paginated and filtered events (AC: #4)
- [x] Handle cases where no events are found (return empty list, 200 OK) (AC: #5)
- [x] Ensure query parameter naming (`pageNumber`, `pageSize`, `type`, `userId`) is `camelCase` (AC: #6)

## Dev Notes

### Relevant architecture patterns and constraints
-   **API Design Pattern:** REST. [Source: architecture.md#Decision: API Design Pattern]
-   **API Naming Conventions:** `kebab-case` for resource paths, `camelCase` for query parameters. [Source: architecture.md#API Naming Conventions (.NET Web API)]
-   **Data Exchange Formats:** `camelCase` for JSON properties, `ISO 8601` for dates. [Source: architecture.md#Data Exchange Formats]
-   **Data Modeling:** Denormalization for CosmosDB. [Source: architecture.md#Decision: Data Modeling Approach for CosmosDB Events]
-   **Code Naming Conventions:** `PascalCase` for C# class/method names, `camelCase` for local variables/public properties. [Source: architecture.md#Code Naming Conventions (Angular SPA, .NET Web API, Azure Function App)]
-   **File Organization:** Controllers in `EventHub.WebApi/Controllers/EventsController.cs`. [Source: architecture.md#Complete Project Directory Structure]

### Source tree components to touch
-   `EventHub.WebApi/Controllers/EventsController.cs` (modify existing)
-   `EventHub.WebApi/Models/Event.cs` (use existing)
-   `EventHub.WebApi/Services/CosmosDbService.cs` (or similar data access service, modify or create if not existing, to include retrieval logic)

### Testing standards summary
-   Unit tests for CosmosDB query logic (pagination, filtering).
-   Integration tests for the `/api/events` endpoint to verify correct request handling with various pagination and filter parameters.
-   Manual testing with Postman/Insomnia to verify correct data retrieval and filtering.

### Project Structure Notes
-   Ensure new data access logic aligns with existing or proposed data access patterns.

### References
-   [Source: epics.md#Story 4.1: Implement GET /api/events Endpoint for Paginated and Filterable Events]
-   [Source: prd.md#Functional Requirements]
-   [Source: prd.md#User Journey: Viewing Events]
-   [Source: architecture.md#API & Communication Patterns]
-   [Source: architecture.md#Data Architecture]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Flash

### Debug Log References

### Completion Notes List

Ultimate context engine analysis completed - comprehensive developer guide created

- Implemented `GET /api/events` with `pageNumber`, `pageSize`, `type`, and `userId` query parameters; response body is `PagedEventsResponse` (`items`, `pageNumber`, `pageSize`, `totalCount`) with camelCase JSON.
- `CosmosDbEventListReader` runs parameterized cross-partition queries (COUNT + paged SELECT with `OFFSET`/`LIMIT`) when `CosmosDb:ConnectionString` is set; otherwise `NoOpEventListReader` returns an empty page (Testing/local without Cosmos).
- Invalid `type` filter returns `400` with `application/problem+json` validation errors on `type`.
- Added unit tests for `EventCosmosQueryBuilder` and integration tests for the GET endpoint using `CapturingEventListReader`.

### File List

- EventHub.WebApi/Controllers/EventsController.cs
- EventHub.WebApi/Models/PagedEventsResponse.cs
- EventHub.WebApi/Program.cs
- EventHub.WebApi/Services/IEventListReader.cs
- EventHub.WebApi/Services/NoOpEventListReader.cs
- EventHub.WebApi/Services/CosmosDbEventListReader.cs
- EventHub.WebApi/Services/EventCosmosQueryBuilder.cs
- EventHub.WebApi/Services/EventTypeFilterParser.cs
- EventHub.WebApi.Tests/CapturingEventListReader.cs
- EventHub.WebApi.Tests/EventCosmosQueryBuilderTests.cs
- EventHub.WebApi.Tests/EventsGetTests.cs
- EventHub.WebApi.Tests/TestWebApplicationFactory.cs
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/implementation-artifacts/4-1-implement-get-api-events-endpoint-for-paginated-and-filterable-events.md

### Change Log

- 2026-03-19: Story 4.1 — GET `/api/events` with Cosmos-backed pagination and filters; tests and sprint status → review.