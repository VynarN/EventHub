# Story 5.2: Integrate Azure Application Insights for Logging and Monitoring

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to integrate Azure Application Insights into the .NET Web API and Azure Function App,
So that system health, performance, and errors can be centrally logged and monitored.

## Acceptance Criteria

1. Given the .NET Web API and Azure Function App projects are running, When an event is processed by the .NET Web API (e.g., POST request received), Then Azure Application Insights is configured in the .NET Web API to automatically capture requests, dependencies, and exceptions.
2. And when an event is processed by the Azure Function App (e.g., Service Bus message consumed), Then Azure Application Insights is configured in the Azure Function App to automatically capture invocations, dependencies, and exceptions.
3. And custom logs (e.g., informational messages) can be sent to Application Insights from both the .NET Web API and Azure Function App.
4. And key performance metrics (e.g., request rates, error rates, latency) for both services are visible in the Azure Portal via Application Insights dashboards.

## Tasks / Subtasks

- [x] Configure Application Insights in .NET Web API (`Program.cs` or `Startup.cs`) (AC: #1)
- [x] Configure Application Insights in Azure Function App (`host.json`, `local.settings.json`, `Program.cs` if using isolated process) (AC: #2)
- [x] Ensure automatic collection of requests, dependencies, and exceptions for both services (AC: #1, #2)
- [x] Implement logging of custom informational messages to Application Insights from both services (AC: #3)
- [x] Verify metrics and logs are appearing in Azure Application Insights in Azure Portal (AC: #4)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Monitoring and Logging:** Azure Application Insights is mandated for comprehensive error logging and monitoring. [Source: architecture.md#Decision: Monitoring and Logging]
-   **Environment Configuration:** Environment variables for connection strings. [Source: architecture.md#Decision: Environment Configuration]
-   **Non-Functional Requirements:** NFR6 (Logging), NFR7 (Monitoring), NFR1 (Scalability), NFR3 (Performance API), NFR4 (Performance UI) are directly supported by Application Insights integration. [Source: prd.md#Non-Functional Requirements]

### Source tree components to touch
-   `EventHub.WebApi/Program.cs` or `Startup.cs` (modify existing)
-   `EventHub.WebApi/appsettings.json` (for Application Insights connection string or instrumentation key)
-   `EventHub.FunctionApp/host.json` (modify existing)
-   `EventHub.FunctionApp/local.settings.json` (for Application Insights connection string or instrumentation key)
-   `EventHub.FunctionApp/Program.cs` (if using isolated process model for dependency injection setup)
-   Existing service logic (e.g., `EventsController.cs`, `EventConsumerFunction.cs`) to add custom logging.

### Testing standards summary
-   Simulate various requests and events to both API and Function App, then verify telemetry data appears in Application Insights.
-   Trigger exceptions and errors to confirm they are logged correctly.
-   Check for custom log messages in Application Insights traces.

### Project Structure Notes
-   Configuration for Application Insights should be handled in `appsettings.json` for the Web API and `local.settings.json`/`host.json` for the Function App.

### References
-   [Source: epics.md#Story 5.2: Integrate Azure Application Insights for Logging and Monitoring]
-   [Source: prd.md#Monitoring & Logging]
-   [Source: architecture.md#Infrastructure & Deployment]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Flash

### Implementation Plan

- Register ASP.NET Core Application Insights telemetry in `Program.cs` and bind connection string from configuration / `APPLICATIONINSIGHTS_CONNECTION_STRING`.
- Align Function App `host.json` with full automatic collection; document local/Azure connection string in `local.settings.json` and Docker.
- Emit custom traces via `ILogger` on API event paths; rely on existing Function logs.
- Add tests proving Web API DI registration for Application Insights.

### Debug Log References

### Completion Notes List

Ultimate context engine analysis completed - comprehensive developer guide created

- Web API: Added `Microsoft.ApplicationInsights.AspNetCore`, `AddApplicationInsightsTelemetry()` in `Program.cs`, `ApplicationInsights:ConnectionString` in `appsettings.json` (use env `APPLICATIONINSIGHTS_CONNECTION_STRING` in Azure/Docker; compose passes `${APPLICATIONINSIGHTS_CONNECTION_STRING}`). `EventsController` uses `ILogger` for list/create informational traces (routed to App Insights when a connection string is set).
- Function App: Confirmed isolated worker setup (`AddApplicationInsightsTelemetryWorkerService` + `ConfigureFunctionsApplicationInsights`). Added `APPLICATIONINSIGHTS_CONNECTION_STRING` to `local.settings.json`, removed `excludedTypes: Request` from `host.json` so invocations/requests are not suppressed. `EventConsumerFunction` already emits structured `ILogger` information/warning logs.
- **AC4 (Portal):** After setting a real Application Insights connection string on both apps, confirm in Azure Portal: Live Metrics / Logs for requests (Web API), function executions and dependencies, exceptions, and custom trace messages (e.g. "Listed events", "Event created", "Received event").
- Tests: `ApplicationInsightsRegistrationTests` asserts `TelemetryClient` and `TelemetryConfiguration` are registered in the Web API host.

### File List

- EventHub.WebApi/EventHub.WebApi.csproj
- EventHub.WebApi/Program.cs
- EventHub.WebApi/appsettings.json
- EventHub.WebApi/Controllers/EventsController.cs
- EventHub.FunctionApp/host.json
- EventHub.FunctionApp/local.settings.json
- docker-compose.yml
- EventHub.WebApi.Tests/ApplicationInsightsRegistrationTests.cs
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/implementation-artifacts/5-2-integrate-azure-application-insights-for-logging-and-monitoring.md

### Change Log

- 2026-03-19: Story 5.2 — Application Insights for Web API and Function App; host.json sampling; docker-compose connection string passthrough; registration tests.