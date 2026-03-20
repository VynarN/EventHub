# Story 2.4: Implement Event Creation Form in Angular SPA

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a user,
I want to create new events through a reactive form in the Angular SPA,
So that I can easily submit event data to the EventHub system.

## Acceptance Criteria

1. Given the Angular SPA project is running, and the POST `/api/events` endpoint is available (Story 2.2), When I navigate to the event creation section of the Angular SPA, Then an `EventCreateComponent` (or similar) is created in `eventhub-spa/src/app/events/components/event-create/`.
2. And the component displays a reactive form with input fields for `UserId` (text), `Type` (dropdown with PageView, Click, Purchase options), and `Description` (textarea).
3. And the form includes appropriate client-side validation (e.g., required fields).
4. And upon successful submission, the Angular SPA sends a POST request to `/api/events` with the `EventCreationDto` payload.
5. And the UI provides visual feedback (e.g., a success message, clearing the form) upon successful event creation.
6. And the UI displays user-friendly error messages for failed submissions (e.g., API errors, validation errors).

## Tasks / Subtasks

- [x] Generate `EventCreateComponent` (AC: #1)
- [x] Implement reactive form in `EventCreateComponent` (AC: #2)
  - [x] Add `UserId` input (AC: #2)
  - [x] Add `Type` dropdown with options (AC: #2)
  - [x] Add `Description` textarea (AC: #2)
- [x] Implement client-side validation for form fields (AC: #3)
- [x] Implement form submission logic to send POST request to `/api/events` with `EventCreationDto` (AC: #4)
- [x] Display success message and clear form on successful submission (AC: #5)
- [x] Display user-friendly error messages on failed submission (AC: #6)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Component Architecture:** Feature-based Modules/Components with Smart/Dumb Component principles. [Source: architecture.md#Decision: Component Architecture]
-   **State Management:** Simple service with RxJS or component-local state. [Source: architecture.md#Decision: State Management Approach]
-   **Code Naming Conventions:** `PascalCase` for Angular component class names, `kebab-case` for selectors and file names. `PascalCase` for TypeScript interface/type names, `camelCase` for TypeScript function/method names. [Source: architecture.md#Code Naming Conventions (Angular SPA, .NET Web API, Azure Function App)]
-   **Error Handling Patterns:** Global `ErrorHandler` for Angular, consistent user-friendly messages. [Source: architecture.md#Error Handling Patterns]
-   **Loading State Patterns:** Clear boolean flags, consistent visual feedback. [Source: architecture.md#Loading State Patterns]
-   **File Organization:** Components in `eventhub-spa/src/app/events/components/event-create/`. [Source: architecture.md#Complete Project Directory Structure]

### Source tree components to touch
-   `eventhub-spa/src/app/events/components/event-create/event-create.component.ts` (new file)
-   `eventhub-spa/src/app/events/components/event-create/event-create.component.html` (new file)
-   `eventhub-spa/src/app/events/components/event-create/event-create.component.scss` (new file)
-   `eventhub-spa/src/app/events/events.module.ts` (update to declare and export `EventCreateComponent`)
-   `eventhub-spa/src/app/events/events-routing.module.ts` (add route for event creation)
-   `eventhub-spa/src/app/shared/models/event.model.ts` (use existing for `Type` enum values)
-   `eventhub-spa/src/app/shared/models/event-creation.dto.ts` (use existing)

### Testing standards summary
-   Unit tests for `EventCreateComponent` to verify form initialization, validation, and submission logic.
-   End-to-end tests to simulate user interaction with the form and verify successful event creation and UI feedback.
-   Manual testing of the form in the browser, including valid and invalid submissions.

### Project Structure Notes
-   Ensure the new component files are placed in `eventhub-spa/src/app/events/components/event-create/`.
-   Update the `events.module.ts` to include the new component.

### References
-   [Source: epics.md#Story 2.4: Implement Event Creation Form in Angular SPA]
-   [Source: prd.md#Functional Requirements]
-   [Source: prd.md#User Journey: Creating an Event]
-   [Source: architecture.md#Frontend Architecture]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Flash

### Debug Log References

### Completion Notes List

- Implemented standalone `EventCreateComponent` at `eventhub-spa/src/app/events/components/event-create/` with reactive form (`userId`, `type`, `description`), required validation, POST to `/api/events` using `EventCreationDto`, success banner + reset, and API error parsing (validation `errors`, then RFC 9457-style `detail` / `title`).
- SPA uses standalone routing (no `NgModule`); registered route `events/create` and default redirect from `''`; app shell with nav link. Added `API_BASE_URL` injection token so browser traffic from port 4200 targets `http://<host>:5080` for cross-origin API calls.
- Development-only CORS on Web API for `http://localhost:4200` so Docker/local SPA can call the API.
- Unit tests: `EventCreateComponent` (HttpTestingController) and updated `App` spec for routed outlet.

### File List

- eventhub-spa/src/app/core/api-base-url.token.ts
- eventhub-spa/src/app/events/components/event-create/event-create.component.ts
- eventhub-spa/src/app/events/components/event-create/event-create.component.html
- eventhub-spa/src/app/events/components/event-create/event-create.component.scss
- eventhub-spa/src/app/events/components/event-create/event-create.component.spec.ts
- eventhub-spa/src/app/app.config.ts
- eventhub-spa/src/app/app.routes.ts
- eventhub-spa/src/app/app.ts
- eventhub-spa/src/app/app.html
- eventhub-spa/src/app/app.scss
- eventhub-spa/src/app/app.spec.ts
- EventHub.WebApi/Program.cs
- _bmad-output/implementation-artifacts/2-4-implement-event-creation-form-in-angular-spa.md
- _bmad-output/implementation-artifacts/sprint-status.yaml

### Change Log

- 2026-03-19: Story 2.4 — event creation form, API base URL token, dev CORS, tests (Date: 2026-03-19)