import { describe, expect, it } from 'vitest';
import { EventType, type Event } from './event.model';
import type { EventCreationDto } from './event-creation.dto';

describe('event models', () => {
  it('serializes Event to camelCase JSON keys', () => {
    const evt: Event = {
      id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
      userId: 'user-1',
      type: EventType.PageView,
      description: 'Home',
      createdAt: '2026-03-19T12:00:00.000Z',
    };
    const json = JSON.stringify(evt);
    const parsed = JSON.parse(json) as Record<string, unknown>;
    expect(Object.keys(parsed).sort()).toEqual(
      ['createdAt', 'description', 'id', 'type', 'userId'].sort(),
    );
    expect(parsed['type']).toBe('pageView');
  });

  it('EventCreationDto uses camelCase-compatible fields', () => {
    const dto: EventCreationDto = {
      userId: 'u42',
      type: EventType.Click,
      description: 'cta',
    };
    expect(JSON.parse(JSON.stringify(dto))).toEqual({
      userId: 'u42',
      type: 'click',
      description: 'cta',
    });
  });
});
