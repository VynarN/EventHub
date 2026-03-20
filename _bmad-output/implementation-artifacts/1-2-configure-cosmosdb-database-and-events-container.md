# Story 1.2: Configure CosmosDB Database and Events Container

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to set up the CosmosDB database and an `Events` container with the `Id` field as the partition key,
So that the system has a persistent and properly partitioned storage for event data.

## Acceptance Criteria

1. Given the local development environment is running with the CosmosDB emulator, When the .NET Web API or Azure Function App attempts to connect to CosmosDB, Then an `EventHub` database is created if it doesn't already exist.
2. And an `Events` container is created within the `EventHub` database if it doesn't already exist.
3. And the `Events` container is configured to use the `Id` field as its partition key.
4. And I can successfully write and read data to the `Events` container via the CosmosDB emulator.

## Tasks / Subtasks

- [x] Implement CosmosDB client configuration in .NET Web API (AC: #1)
- [x] Implement CosmosDB client configuration in Azure Function App (AC: #1)
- [x] Develop logic to create `EventHub` database if it doesn't exist (AC: #1)
- [x] Develop logic to create `Events` container if it doesn't exist, with `Id` as partition key (AC: #2, #3)
- [x] Create a simple test to write and read data to the `Events` container using the emulator (AC: #4)

## Dev Notes

### Relevant architecture patterns and constraints
-   **CosmosDB Naming Conventions:** Container Naming: `PascalCase` and singular (e.g., `Events`); Property Naming: `camelCase` (e.g., `userId`, `createdAt`); Partition Key: `Id` field. [Source: architecture.md#Database Naming Conventions (Azure CosmosDB)]
-   **Data Modeling:** Denormalization is chosen for read-heavy event storage, with `Id` as the partition key. [Source: architecture.md#Decision: Data Modeling Approach for CosmosDB Events]
-   **Local Development:** Use the CosmosDB emulator provided by `docker-compose.yml` for local testing. [Source: prd.md#Local Development Environment]

### Source tree components to touch
-   `EventHub.WebApi/`: Potentially add a data access layer or configuration for CosmosDB connection.
-   `EventHub.FunctionApp/`: Potentially add a data access layer or configuration for CosmosDB connection.
-   Configuration files (e.g., `appsettings.json`, `local.settings.json`) to store CosmosDB connection strings and database/container names.

### Testing standards summary
-   Manual verification using CosmosDB emulator data explorer.
-   Automated integration tests that connect to the emulator and perform CRUD operations to confirm correct setup and partitioning.

### Project Structure Notes
-   Ensure any new data access components or configuration files are placed according to `architecture.md#File Organization Patterns`.

### References
-   [Source: epics.md#Story 1.2: Configure CosmosDB Database and Events Container]
-   [Source: prd.md#Azure CosmosDB]
-   [Source: architecture.md#Data Architecture]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Composer (dev-story workflow)

### Debug Log References

### Completion Notes List

- Added shared `EventHub.Cosmos` library: `CosmosClient` in gateway mode, optional TLS bypass for local HTTPS emulator, `CreateDatabaseIfNotExistsAsync` + `CreateContainerIfNotExistsAsync` with partition path `/Id`, hosted service for startup provisioning.
- Wired Web API and Azure Functions via `AddEventHubCosmos`; Web API uses `appsettings.Development.json` for localhost emulator, `appsettings.Testing.json` clears connection string so `WebApplicationFactory` tests do not require Cosmos.
- Docker Compose passes emulator connection string to `eventhub-webapi` and `eventhub-functions`; build context is repo root with multi-project Dockerfiles.
- Integration test `Events_container_supports_write_and_read_roundtrip` skips when emulator is unreachable; passes when emulator is up.

### File List

- EventHub.Cosmos/CosmosDbSettings.cs
- EventHub.Cosmos/CosmosInfrastructure.cs
- EventHub.Cosmos/EventHub.Cosmos.csproj
- EventHub.Cosmos/ServiceCollectionExtensions.cs
- EventHub.FunctionApp/appsettings.json
- EventHub.FunctionApp/appsettings.Development.json
- EventHub.FunctionApp/Dockerfile
- EventHub.FunctionApp/EventHub.FunctionApp.csproj
- EventHub.FunctionApp/local.settings.json.example
- EventHub.FunctionApp/Program.cs
- EventHub.WebApi/appsettings.json
- EventHub.WebApi/appsettings.Development.json
- EventHub.WebApi/appsettings.Testing.json
- EventHub.WebApi/Dockerfile
- EventHub.WebApi/EventHub.WebApi.csproj
- EventHub.WebApi/Program.cs
- EventHub.WebApi.Tests/EventsContainerCosmosTests.cs
- EventHub.WebApi.Tests/EventHub.WebApi.Tests.csproj
- EventHub.WebApi.Tests/TestConfiguration.cs
- EventHub.WebApi.Tests/TestWebApplicationFactory.cs
- EventHub.WebApi.Tests/WeatherForecastTests.cs
- .dockerignore
- docker-compose.yml
- README.md
- _bmad-output/implementation-artifacts/1-2-configure-cosmosdb-database-and-events-container.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-03-19: Story 1.2 — Cosmos DB client, EventHub DB + Events container bootstrap, compose/Docker wiring, emulator integration test (skippable).