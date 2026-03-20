# Story 5.1: Implement RFC 9457 Compliant Error Handling in .NET Web API

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to implement RFC 9457 (Problem Details for HTTP APIs) compliant error handling in the .NET Web API,
So that API clients receive consistent, machine-readable error responses, improving API usability and debuggability.

## Acceptance Criteria

1. Given the .NET Web API is running, When an API endpoint encounters a known error (e.g., validation failure, resource not found), Then the API returns an error response adhering to the RFC 9457 Problem Details JSON format.
2. And the error response includes `type`, `title`, `status`, `detail`, and `instance` fields as defined by RFC 9457.
3. And specific HTTP status codes (e.g., 400 Bad Request, 404 Not Found, 500 Internal Server Error) are mapped correctly to the `status` field.
4. And the `detail` field provides a human-readable explanation of the error.
5. And a global exception handling middleware is implemented to catch unhandled exceptions and return RFC 9457 compliant 500 Internal Server Error responses.

## Tasks / Subtasks

- [x] Research and select appropriate .NET libraries or built-in features for RFC 9457 compliance (AC: #1)
- [x] Implement a custom exception filter or middleware for global exception handling (AC: #5)
- [x] Configure the middleware to produce RFC 9457 compliant responses for unhandled exceptions (AC: #1, #2, #3, #4)
- [x] Implement custom problem details for specific known error scenarios (e.g., validation errors in DTOs) (AC: #1, #2, #3, #4)
- [x] Ensure correct HTTP status code mapping (AC: #3)
- [x] Provide meaningful `detail` messages (AC: #4)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Error Handling Standard:** RFC 9457 (Problem Details for HTTP APIs) is mandated. [Source: architecture.md#Decision: Error Handling Standard]
-   **API Response Formats:** Adherence to RFC 9457 for error formats. [Source: architecture.md#API Response Formats]
-   **Data Exchange Formats:** `camelCase` for all JSON properties. [Source: architecture.md#Data Exchange Formats]
-   **Process Patterns:** Global error handling middleware for .NET Web API. [Source: architecture.md#Error Handling Patterns]

### Source tree components to touch
-   `EventHub.WebApi/Program.cs` or `Startup.cs` (for middleware registration)
-   `EventHub.WebApi/Filters/ProblemDetailsFilter.cs` (or similar custom filter, new file)
-   `EventHub.WebApi/Models/ProblemDetails.cs` (or use built-in .NET types, new file if custom)
-   Existing controllers (e.g., `EventsController.cs`) may need minor adjustments to throw specific exceptions that the middleware can catch.

### Testing standards summary
-   Unit tests for custom exception filters/middleware to ensure correct RFC 9457 response generation for various exception types.
-   Integration tests for API endpoints to verify error responses for invalid inputs (e.g., sending an `EventCreationDto` with missing required fields) and server-side errors.
-   Manual testing with Postman/Insomnia to trigger different error scenarios and inspect the response format.

### Project Structure Notes
-   New filter or model files should be placed in appropriate directories like `Filters/` or `Models/` within `EventHub.WebApi/`.

### References
-   [Source: epics.md#Story 5.1: Implement RFC 9457 Compliant Error Handling in .NET Web API]
-   [Source: prd.md#Error Handling]
-   [Source: architecture.md#API & Communication Patterns]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Flash

### Debug Log References

### Completion Notes List

- Used ASP.NET Core built-ins: `AddProblemDetails`, `IProblemDetailsService`, and `IExceptionHandler` (`Rfc9457ExceptionHandler`) with `UseExceptionHandler` for global unhandled exceptions (500, RFC 9457 fields including `type` for HTTP 500 per RFC 9110).
- `ConfigureApiBehaviorOptions.InvalidModelStateResponseFactory` returns `ValidationProblemDetails` with explicit `title`, `detail`, and `instance` (camelCase JSON aligns with existing API options).
- Manual validation for invalid `type` query on `GET /api/events` now includes `title`, `detail`, and `instance`.
- Integration tests: `ExceptionHandlingTests` covers 500 problem+json via `ThrowingEventPublisher`, and validation shape for `title`/`detail`/`instance`.

### File List

- EventHub.WebApi/ExceptionHandling/Rfc9457ExceptionHandler.cs
- EventHub.WebApi/Program.cs
- EventHub.WebApi/Controllers/EventsController.cs
- EventHub.WebApi.Tests/ThrowingEventPublisher.cs
- EventHub.WebApi.Tests/ExceptionHandlingTests.cs
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/implementation-artifacts/5-1-implement-rfc-9457-compliant-error-handling-in-net-web-api.md

## Change Log

- 2026-03-19: RFC 9457 global exception handling, validation problem details customization, integration tests (Story 5.1).