# Story 2.1: Define Event Model and Data Transfer Objects (DTOs)

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want to define the core Event model and associated Data Transfer Objects for event creation and internal representation,
So that event data can be consistently structured and communicated between the Angular SPA, .NET Web API, Azure Function App, and CosmosDB.

## Acceptance Criteria

1. Given the project setup from Epic 1, When I define the event structures, Then an `Event` model is created in the .NET Web API (`EventHub.WebApi/Models/Event.cs`) with properties for `Id` (GUID), `UserId` (string), `Type` (enum: PageView, Click, Purchase), `Description` (string), and `CreatedAt` (DateTime).
2. And an equivalent `Event` interface or class is defined in the Angular SPA (`eventhub-spa/src/app/shared/models/event.model.ts`) with corresponding types.
3. And an `EventCreationDto` (or similar) is defined in the .NET Web API for incoming POST requests, containing `UserId`, `Type`, and `Description`.
4. And an equivalent DTO is defined in the Angular SPA for sending event creation requests.
5. And all property names in DTOs and models adhere to the `camelCase` for JSON and `PascalCase` for C# conventions as per the Architecture document.
6. And the `Type` enum values are correctly defined and used.

## Tasks / Subtasks

- [x] Create `Event` model in `EventHub.WebApi/Models/Event.cs` (AC: #1)
  - [x] Define `Id` as GUID
  - [x] Define `UserId` as string
  - [x] Define `Type` as enum (PageView, Click, Purchase)
  - [x] Define `Description` as string
  - [x] Define `CreatedAt` as DateTime
- [x] Create `Event` interface/class in `eventhub-spa/src/app/shared/models/event.model.ts` (AC: #2)
  - [x] Define corresponding types for properties
- [x] Create `EventCreationDto` in .NET Web API for POST requests (AC: #3)
  - [x] Include `UserId`, `Type`, `Description`
- [x] Create equivalent DTO in Angular SPA for sending requests (AC: #4)
- [x] Ensure `camelCase` for JSON and `PascalCase` for C# property names (AC: #5)
- [x] Define and use `Type` enum values correctly (AC: #6)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Data Modeling:** Denormalization is chosen for event storage. [Source: architecture.md#Decision: Data Modeling Approach for CosmosDB Events]
-   **Naming Conventions:** `camelCase` for JSON properties, `PascalCase` for C# class/interface/method names, `camelCase` for TypeScript function/method names, `PascalCase` for TypeScript interface/type names. [Source: architecture.md#Code Naming Conventions (Angular SPA, .NET Web API, Azure Function App)]
-   **File Organization:** Event model in `EventHub.WebApi/Models/Event.cs`, Angular event interface/class in `eventhub-spa/src/app/shared/models/event.model.ts`. [Source: architecture.md#Complete Project Directory Structure]
-   **Data Exchange Formats:** `camelCase` for JSON properties, `true`/`false` for booleans, explicit `null` for absent data, `ISO 8601` for dates. [Source: architecture.md#Data Exchange Formats]

### Source tree components to touch
-   `EventHub.WebApi/Models/Event.cs` (new file)
-   `EventHub.WebApi/Models/EventCreationDto.cs` (new file)
-   `eventhub-spa/src/app/shared/models/event.model.ts` (new file)
-   `eventhub-spa/src/app/shared/models/event-creation.dto.ts` (new file)

### Testing standards summary
-   Compile-time checks for correct type definitions.
-   Serialization/deserialization tests for DTOs to ensure correct JSON formatting and naming conventions.

### Project Structure Notes
-   New model and DTO files should be placed in the `Models` folder for .NET Web API and `shared/models` for Angular SPA, adhering to the defined project structure. [Source: architecture.md#Complete Project Directory Structure]

### References
-   [Source: epics.md#Story 2.1: Define Event Model and Data Transfer Objects (DTOs)]
-   [Source: prd.md#Functional Requirements]
-   [Source: architecture.md#Data Architecture]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Flash

### Debug Log References

### Completion Notes List

Ultimate context engine analysis completed - comprehensive developer guide created

- Implemented `Event`, `EventType`, and `EventCreationDto` in Web API; `ConfigureHttpJsonOptions` uses camelCase property names and camelCase string enums to match architecture.
- Added Angular `Event`, `EventType` string enum (`pageView` / `click` / `purchase`), and `EventCreationDto`; Vitest specs assert camelCase JSON keys.
- Added `EventModelJsonTests` (xUnit) for serialization and DTO round-trip.

### File List

- EventHub.WebApi/Models/EventType.cs
- EventHub.WebApi/Models/Event.cs
- EventHub.WebApi/Models/EventCreationDto.cs
- EventHub.WebApi/Program.cs
- EventHub.WebApi.Tests/EventModelJsonTests.cs
- eventhub-spa/src/app/shared/models/event.model.ts
- eventhub-spa/src/app/shared/models/event-creation.dto.ts
- eventhub-spa/src/app/shared/models/event.model.spec.ts

## Change Log

- 2026-03-19: Story 2.1 — event model/DTOs (.NET + Angular), JSON naming tests, minimal API JSON options.