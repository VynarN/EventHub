# Story 1.1: Initialize Core Projects and Local Development Environment

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to set up the foundational projects for the Angular SPA, .NET Web API, and a local development environment with Azure emulators,
So that I have a runnable baseline for all application components and their dependencies locally.

## Acceptance Criteria

1. Given I have Docker installed, When I execute the initialization commands and `docker-compose up`, Then the Angular SPA project is created using `ng new eventhub-spa --directory eventhub-spa --routing --style=scss`.
2. And the .NET Web API project is created using `dotnet new webapi -n EventHub.WebApi`.
3. And a `docker-compose.yml` file is created and configured to orchestrate the Angular SPA, .NET Web API, Azure Function App, Azure CosmosDB emulator, and Azure Service Bus emulator.
4. And all services start successfully via `docker-compose up`.
5. And the Angular SPA is accessible in the browser.
6. And the .NET Web API is accessible.
7. And the CosmosDB and Service Bus emulators are running and accessible by their respective services.

## Tasks / Subtasks

- [x] Initialize Angular SPA project (AC: #1)
- [x] Initialize .NET Web API project (AC: #2)
- [x] Create initial `docker-compose.yml` (AC: #3)
- [x] Configure `docker-compose.yml` for Angular SPA (AC: #3, #5)
- [x] Configure `docker-compose.yml` for .NET Web API (AC: #3, #6)
- [x] Configure `docker-compose.yml` for Azure Function App (AC: #3)
- [x] Configure `docker-compose.yml` for Azure CosmosDB emulator (AC: #3, #7)
- [x] Configure `docker-compose.yml` for Azure Service Bus emulator (AC: #3, #7)
- [x] Verify all services start successfully (AC: #4)
- [x] Verify Angular SPA accessibility (AC: #5)
- [x] Verify .NET Web API accessibility (AC: #6)
- [x] Verify CosmosDB and Service Bus emulator accessibility (AC: #7)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Project Initialization:** Use official templates for .NET Web API (`dotnet new webapi -n EventHub.WebApi`) and Angular SPA (`ng new eventhub-spa --directory eventhub-spa --routing --style=scss`). [Source: architecture.md#Selected Starter: Official Templates for .NET Web API and Angular SPA]
-   **Containerization:** All application components (Angular SPA, .NET Web API, Azure Function App) shall be containerized using Docker. [Source: prd.md#Containerization]
-   **Local Development Environment:** A `docker-compose.yml` file shall be provided to orchestrate local development, including emulators for Azure CosmosDB and Azure Service Bus. [Source: prd.md#Local Development Environment]
-   **Directory Structure:** Adhere to the defined `Project Directory Structure` as outlined in the Architecture Decision Document. [Source: architecture.md#Complete Project Directory Structure]

### Source tree components to touch
-   Create `eventhub-spa/` directory and project files.
-   Create `EventHub.WebApi/` directory and project files.
-   Create `EventHub.FunctionApp/` directory and basic project files (to be elaborated in future stories).
-   Create `docker-compose.yml` at the project root.

### Testing standards summary
-   Initial setup verification will involve ensuring all containerized services start and are accessible locally. No unit/integration tests are expected at this stage, but accessibility should be confirmed.

### Project Structure Notes
-   The newly created projects (`eventhub-spa`, `EventHub.WebApi`, `EventHub.FunctionApp`) and `docker-compose.yml` must align with the `EventHub` root directory structure as described in `architecture.md#Complete Project Directory Structure`.

### References
-   [Source: epics.md#Story 1.1: Initialize Core Projects and Local Development Environment]
-   [Source: prd.md#Minimum Viable Product (MVP)]
-   [Source: prd.md#Local Development Environment]
-   [Source: architecture.md#Selected Starter: Official Templates for .NET Web API and Angular SPA]
-   [Source: architecture.md#Complete Project Directory Structure]

## Dev Agent Record

### Agent Model Used

(Composer implementation session)

### Debug Log References

- Angular `ng new` initially failed with `ENOSPC` until npm cache was redirected to `D:\npm-cache` (C: drive full on dev machine).

### Implementation Plan

- Scaffold `EventHub.WebApi` with `dotnet new webapi`, `EventHub.FunctionApp` with `dotnet new func` (net8.0, isolated worker), and `eventhub-spa` with Angular CLI (`--routing`, `--style=scss`, non-interactive flags).
- Add per-project Dockerfiles and root `docker-compose.yml` wiring SPA (`ng serve`), Web API, Functions host, Cosmos DB Linux emulator (`vnext-preview`, HTTPS), SQL Edge + Service Bus emulator per Microsoft installer pattern.
- Add `GET /api/health/dependencies` on the Web API to confirm TCP reachability to `cosmos-emulator:8081` and `servicebus-emulator:5672` from the API container.
- Add minimal `EventHub.WebApi.Tests` xUnit + `WebApplicationFactory` smoke test for `/weatherforecast`.

### Completion Notes List

- Root `README.md` documents copy `.env.example` → `.env`, `docker compose up --build`, host ports, and dependency health URL.
- Service Bus emulator uses `docker/servicebus-config.json` (valid JSON, simplified queue-only config).
- Azure Functions include HTTP `GET /api/health` for container smoke checks.
- **Verification:** Run `docker compose up --build` after creating `.env` with a strong `SQL_PASSWORD`. Confirm `http://localhost:4200`, `http://localhost:5080/swagger`, `http://localhost:7071/api/health`, `http://localhost:5080/api/health/dependencies` (true/true after emulators listen), Cosmos explorer `http://localhost:1234`, Service Bus management `http://localhost:5300`. Run `dotnet test EventHub.WebApi.Tests/EventHub.WebApi.Tests.csproj` on the host.

### File List

- `README.md`
- `.gitignore`
- `.env.example`
- `docker-compose.yml`
- `docker/servicebus-config.json`
- `eventhub-spa/` (Angular CLI scaffold + `Dockerfile`, `.dockerignore`)
- `EventHub.WebApi/` (template + `Dockerfile`, `.dockerignore`, `Program.cs` updates)
- `EventHub.FunctionApp/` (template + `Dockerfile`, `.dockerignore`, `HealthFunction.cs`)
- `EventHub.WebApi.Tests/` (`EventHub.WebApi.Tests.csproj`, `WeatherForecastTests.cs`)

## Change Log

- 2026-03-19: Story 1.1 — Scaffolded SPA, Web API, Functions; Docker Compose with Cosmos + Service Bus emulators; health endpoints; Web API smoke test; root README and env template.
