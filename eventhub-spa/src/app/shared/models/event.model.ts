/** String values align with .NET `JsonStringEnumConverter` using camelCase. */
export enum EventType {
  PageView = 'pageView',
  Click = 'click',
  Purchase = 'purchase',
}

/** Full event shape (API / Cosmos). */
export interface Event {
  id: string;
  userId: string;
  type: EventType;
  description: string;
  /** ISO 8601 from API */
  createdAt: string;
}

/** GET /api/events paged response (camelCase from ASP.NET Core JSON). */
export interface PagedEventsResponse {
  items: Event[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

/** Filter criteria for the event list (persisted in session for AC #5). */
export interface EventListFilterCriteria {
  type: EventType | null;
  userId: string;
  /** Inclusive lower bound, ISO 8601 UTC, or empty */
  createdFrom: string;
  /** Inclusive upper bound, ISO 8601 UTC, or empty */
  createdTo: string;
}
