# Story 4.2: Implement Event List Data Grid in Angular SPA

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a user,
I want to view a paginated list of events in a data grid in the Angular SPA,
So that I can easily browse all recorded events.

## Acceptance Criteria

1. Given the Angular SPA project is running, and the GET `/api/events` endpoint for paginated events is available (Story 4.1), When I navigate to the event viewing section of the Angular SPA, Then an `EventListComponent` (or similar) is created in `eventhub-spa/src/app/events/components/event-list/`.
2. And the component displays a data grid that fetches and presents events from the `/api/events` endpoint.
3. And the data grid supports pagination, allowing users to navigate through different pages of events.
4. And the data grid displays key event properties (e.g., `Id`, `UserId`, `Type`, `Description`, `CreatedAt`).
5. And the UI provides visual feedback for loading states while fetching events.
6. And the UI displays user-friendly error messages if event data cannot be retrieved.

## Tasks / Subtasks

- [x] Ensure `EventListComponent` lives under `eventhub-spa/src/app/events/components/event-list/` (AC: #1)
- [x] Bind grid data to `GET /api/events` with `pageNumber` and `pageSize` query params (AC: #2)
- [x] Add pagination controls (rows per page, first/prev/next/last, page X of Y) (AC: #3)
- [x] Show columns Id, UserId, Type, Description, CreatedAt (AC: #4)
- [x] Preserve loading indicator while requests are in flight (AC: #5)
- [x] Surface friendly errors for HTTP/network failures (AC: #6)
- [x] Unit tests for pagination, filter reset to page 1, and request query params (AC: #2–#4)

## Dev Notes

### Relevant architecture patterns and constraints

- **Component Architecture:** Feature-based standalone components. [Source: architecture.md]
- **API Communication:** `EventService.listEvents` with camelCase query params aligned with Story 4.1. [Source: architecture.md#External Integrations]
- **State Management:** Filter criteria via `EventListFilterStateService`; pagination via `BehaviorSubject` + `toSignal` to avoid duplicate GETs when filters change. [Source: architecture.md]

### Source tree components to touch

- `eventhub-spa/src/app/events/components/event-list/event-list.component.ts`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.html`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.scss`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.spec.ts`
- `eventhub-spa/src/app/events/components/event-filter/event-filter.component.ts` (sync form from `state$` when criteria change externally)
- `eventhub-spa/src/app/events/services/event.service.ts` (unchanged contract; uses existing `listEvents`)

### Testing standards summary

- Unit tests for `EventListComponent` covering initial page params, paging, and filter-driven reset to page 1.
- `npx ng test --no-watch` for the SPA test suite.

### References

- [Source: epics.md#Story 4.2: Implement Event List Data Grid in Angular SPA]
- [Source: prd.md#Functional Requirements]
- [Source: architecture.md#Frontend Architecture]

## Dev Agent Record

### Agent Model Used

Composer (Cursor agent)

### Debug Log References

### Implementation Plan

- Default grid page size `20` (`DEFAULT_EVENT_LIST_PAGE_SIZE`); user-selectable 10 / 20 / 50.
- Pagination driven by `BehaviorSubject` streams combined with `filterState.state$` inside `switchMap`; reset to page 1 when filters change without re-emitting when already on page 1.
- `distinctUntilChanged` on the combined `{ pageNumber, pageSize, type, userId }` tuple before `listEvents` to suppress identical consecutive loads.
- `EventFilterComponent` subscribes to `filterState.state$` and `patchValue(..., { emitEvent: false })` so programmatic/session updates stay aligned with the form (avoids stale debounced `valueChanges` fighting the list).

### Completion Notes List

- Implemented paginated event grid: Id column (ellipsis + title), pager toolbar, summary line with page counts from API metadata.
- Replaced writable-signal + `toObservable` pagination with `BehaviorSubject` + `toSignal` to stabilize `combineLatest` and eliminate duplicate GETs on filter changes.
- Extended `EventFilterComponent` to mirror `EventListFilterStateService` state into the reactive form without emitting `valueChanges`.
- All SPA tests pass (20); WebApi and FunctionApp test projects pass.

### File List

- `eventhub-spa/src/app/events/components/event-list/event-list.component.ts`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.html`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.scss`
- `eventhub-spa/src/app/events/components/event-list/event-list.component.spec.ts`
- `eventhub-spa/src/app/events/components/event-filter/event-filter.component.ts`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/implementation-artifacts/4-2-implement-event-list-data-grid-in-angular-spa.md`

### Change Log

- 2026-03-19: Story 4.2 — paginated data grid, Id column, pager UX, BehaviorSubject pagination, filter form sync; sprint status → review.
