# Story 1.3: Containerize Application Components and Integrate into Docker Compose

Status: in-progress

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to containerize the Angular SPA, .NET Web API, and Azure Function App, and integrate them into the `docker-compose.yml`,
So that all application components are portable and can be easily deployed and managed within the local development environment.

## Acceptance Criteria

1. Given the Angular SPA, .NET Web API, and Azure Function App projects are initialized, When I execute `docker-compose up` after creating the Dockerfiles, Then a Dockerfile exists for the Angular SPA that builds the application.
2. And a Dockerfile exists for the .NET Web API that builds and runs the API.
3. And a Dockerfile exists for the Azure Function App that builds and runs the function.
4. And the `docker-compose.yml` is updated to include services for the Angular SPA, .NET Web API, and Azure Function App, referencing their respective Dockerfiles.
5. And all services (Angular SPA, .NET Web API, Azure Function App, CosmosDB emulator, Service Bus emulator) start successfully.
6. And I can access each application component (Angular SPA, .NET Web API) via their defined ports.
7. And the Azure Function App is running and listening for Service Bus messages.

## Tasks / Subtasks

- [x] Create Dockerfile for Angular SPA (AC: #1)
- [x] Create Dockerfile for .NET Web API (AC: #2)
- [x] Create Dockerfile for Azure Function App (AC: #3)
- [x] Update `docker-compose.yml` to include new services and reference Dockerfiles (AC: #4)
- [ ] Verify `docker-compose up` starts all services successfully (AC: #5)
- [ ] Verify Angular SPA accessibility via its port (AC: #6)
- [ ] Verify .NET Web API accessibility via its port (AC: #6)
- [ ] Verify Azure Function App is running and listening (AC: #7)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Containerization:** All application components shall be containerized using Docker. [Source: prd.md#Containerization]
-   **Local Development Environment:** `docker-compose.yml` orchestrates local development, including emulators. [Source: prd.md#Local Development Environment]
-   **Project Structure:** Dockerfiles and `docker-compose.yml` must align with the `Project Directory Structure`. [Source: architecture.md#Complete Project Directory Structure]
-   **Build Process:** Dockerfiles will define the build process for each component. [Source: architecture.md#Build Process Structure]

### Source tree components to touch
-   `eventhub-spa/Dockerfile` (new file)
-   `EventHub.WebApi/Dockerfile` (new file)
-   `EventHub.FunctionApp/Dockerfile` (new file)
-   `docker-compose.yml` (modify existing)

### Testing standards summary
-   Confirm `docker-compose up` runs without errors.
-   Access application UIs via browser/API clients.
-   Check Azure Function App logs for successful startup and Service Bus listener activation.

### Project Structure Notes
-   Dockerfiles should be placed in the root of their respective project folders (e.g., `eventhub-spa/Dockerfile`).
-   `docker-compose.yml` remains at the project root.

### References
-   [Source: epics.md#Story 1.3: Containerize Application Components and Integrate into Docker Compose]
-   [Source: prd.md#Containerization]
-   [Source: prd.md#Local Development Environment]
-   [Source: architecture.md#Complete Project Directory Structure]
-   [Source: architecture.md#Build Process Structure]

## Dev Agent Record

### Agent Model Used

Flash

### Debug Log References

### Completion Notes List

- Dockerfiles for `eventhub-spa`, `EventHub.WebApi`, and `EventHub.FunctionApp` were already present and align with compose build contexts.
- Updated `docker-compose.yml`: `ServiceBusConnection` for `eventhub-functions`, and `depends_on` for Cosmos + Service Bus emulators so the function host can register the Service Bus trigger against `queue.1` (matches `docker/servicebus-config.json`).
- Added `ServiceBusQueueListenerFunction` and `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` so AC #7 (listening for Service Bus messages) is implementable when compose is up.
- `docker compose --env-file .env.example config` succeeds; `dotnet build` for Web API and Function App succeeds; `dotnet test` for `EventHub.WebApi.Tests` passes (1 skipped integration test).
- **Blocked in this environment:** Docker Engine returned `no valid drivers found` / bad response from daemon, so tasks 5–8 (runtime `docker compose up` and HTTP/queue verification) were not executed here. Run locally: copy `.env.example` to `.env`, then `docker compose up --build`; check SPA on port 4200, API on 5080 (maps to 8080 in container), Functions on 7071; confirm function logs show Service Bus listener registration.

### File List

- `docker-compose.yml`
- `EventHub.FunctionApp/EventHub.FunctionApp.csproj`
- `EventHub.FunctionApp/ServiceBusQueueListenerFunction.cs`
- `EventHub.FunctionApp/local.settings.json`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/implementation-artifacts/1-3-containerize-application-components-and-integrate-into-docker-compose.md`

## Change Log

- 2026-03-19: Wired Service Bus emulator to Function App in compose; added queue trigger listener and Service Bus worker package; validated compose config and .NET build/tests (Docker runtime verification pending local machine).