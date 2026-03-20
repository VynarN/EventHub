import type { EventType } from './event.model';

/** Payload for POST /api/events (camelCase in JSON). */
export interface EventCreationDto {
  userId: string;
  type: EventType;
  description: string;
}
