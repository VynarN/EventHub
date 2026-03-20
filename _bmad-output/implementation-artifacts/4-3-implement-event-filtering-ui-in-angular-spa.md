# Story 4.3: Implement Event Filtering UI in Angular SPA

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a user,
I want to filter the event list by `Type` and `UserId`,
So that I can quickly find specific events of interest.

## Acceptance Criteria

1. Given the event list data grid is implemented (Story 4.2), and the GET `/api/events` endpoint supports filtering (Story 4.1), When I navigate to the event list section, Then an `EventFilterComponent` (or similar) is created in `eventhub-spa/src/app/events/components/event-filter/`.
2. And the component displays input fields for `Type` (dropdown) and `UserId` (text input).
3. And changes to the filter inputs trigger a new GET request to `/api/events` with the corresponding filter parameters.
4. And the event list data grid updates to display the filtered results.
5. And the UI maintains the current filter state (e.g., selected `Type`, entered `UserId`) across refreshes or navigation within the event list section.

## Tasks / Subtasks

- [x] Generate `EventFilterComponent` (AC: #1)
- [x] Implement filter UI with `Type` dropdown and `UserId` input (AC: #2)
- [x] Implement logic to capture filter changes (AC: #3)
- [x] Integrate with `EventService` to send filtered GET requests (AC: #3)
- [x] Update event list data grid with filtered results (AC: #4)
- [x] Implement state management for filter criteria to persist UI state (AC: #5)

## Dev Notes

### Relevant architecture patterns and constraints
-   **Component Architecture:** Feature-based Modules/Components. [Source: architecture.md#Decision: Component Architecture]
-   **State Management:** Simple service with RxJS `BehaviorSubject` for filter state. [Source: architecture.md#Decision: State Management Approach]
-   **Code Naming Conventions:** `PascalCase` for Angular component class names, `kebab-case` for selectors and file names. `PascalCase` for TypeScript interface/type names, `camelCase` for TypeScript function/method names. [Source: architecture.md#Code Naming Conventions (Angular SPA, .NET Web API, Azure Function App)]
-   **File Organization:** Components in `eventhub-spa/src/app/events/components/event-filter/`. [Source: architecture.md#Complete Project Directory Structure]
-   **API Communication:** Use `EventService` to call `GET /api/events` with query parameters. [Source: architecture.md#External Integrations]

### Source tree components to touch
-   `eventhub-spa/src/app/events/components/event-filter/event-filter.component.ts` (new file)
-   `eventhub-spa/src/app/events/components/event-filter/event-filter.component.html` (new file)
-   `eventhub-spa/src/app/events/components/event-filter/event-filter.component.scss` (new file)
-   `eventhub-spa/src/app/events/events.module.ts` (update to declare `EventFilterComponent`)
-   `eventhub-spa/src/app/events/services/event.service.ts` (modify to include filter parameters in GET requests)
-   `eventhub-spa/src/app/events/components/event-list/event-list.component.ts` (modify to receive filtered data)
-   `eventhub-spa/src/app/shared/models/event.model.ts` (use existing for `Type` enum values)

### Testing standards summary
-   Unit tests for `EventFilterComponent` to verify form interaction and event emission.
-   Integration tests to ensure filter changes correctly trigger API calls and update the event list.
-   End-to-end tests to verify persistent filter state and accurate filtering behavior in the UI.

### Project Structure Notes
-   Ensure the new component files are placed in `eventhub-spa/src/app/events/components/event-filter/`.
-   Update the `events.module.ts` to include the new component.

### References
-   [Source: epics.md#Story 4.3: Implement Event Filtering UI in Angular SPA]
-   [Source: prd.md#Functional Requirements]
-   [Source: prd.md#User Journey: Viewing Events]
-   [Source: architecture.md#Frontend Architecture]
-   [Source: architecture.md#Implementation Patterns & Consistency Rules]

## Dev Agent Record

### Agent Model Used

Composer (Cursor agent)

### Debug Log References

### Implementation Plan

- Standalone Angular 21 app (no `NgModule`); declared new components in place and routed `/events` to `EventListComponent`.
- `EventListFilterStateService` holds filter criteria in a `BehaviorSubject`, persists to `sessionStorage` under `eventhub.eventList.filters`, and skips `next` when criteria are unchanged to avoid duplicate GETs.
- `EventFilterComponent` uses reactive forms, 300ms debounce on `valueChanges`, and `distinctUntilChanged` before calling `update`.
- `EventListComponent` embeds the filter, subscribes to filter state with `switchMap` into `EventService.listEvents`, and renders a simple table (minimal list/grid until Story 4.2 assets existed only on paper).

### Completion Notes List

- Implemented `EventFilterComponent` under `events/components/event-filter/` with Type dropdown (including “Any type”) and UserId text input.
- Added `EventService.listEvents` with optional `type` / `userId` query params aligned with `GET /api/events`.
- Added `EventListComponent` with table UI, loading/error/summary states, and integration with filter-driven refetch.
- Filter state survives refresh and navigation within the SPA via `sessionStorage`.
- Unit tests: `EventService`, `EventListFilterStateService`, `EventFilterComponent`, `EventListComponent`; `npx ng test --no-watch` passes (18 tests). .NET test projects unchanged and still pass.

### File List

- `eventhub-spa/src/app/shared/models/event.model.ts`
- `eventhub-spa/src/app/events/services/event.service.ts`
- `eventhub-spa/src/app/events/services/event.service.spec.ts`
- `eventhub-spa/src/app/events/services/event-list-filter-state.service.ts`
- `eventhub-spa/src/app/events/services/event-list-filter-state.service.spec.ts`
- `eventhub-spa/src/app/events/components/event-filter/event-filter.component.ts`
- `eventhub-spa/src/app/events/components/event-filter/event-filter.component.html`
- `eventhub-spa/src/app/events/components/event-filter/event-filter.component.scss`
- `eventhub-spa/src/app/events/components/event-filter/event-filter.component.spec.ts`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.ts`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.html`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.scss`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.spec.ts`
- `eventhub-spa/src/app/app.routes.ts`
- `eventhub-spa/src/app/app.html`
- `eventhub-spa/src/app/app.scss`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/implementation-artifacts/4-3-implement-event-filtering-ui-in-angular-spa.md`

### Change Log

- 2026-03-19: Story 4.3 — event filtering UI, `EventService` list with filters, session-persisted filter state, `/events` route and nav link; sprint status → review.