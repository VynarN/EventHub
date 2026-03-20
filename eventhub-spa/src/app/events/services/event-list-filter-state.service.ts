import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import type { EventListFilterCriteria } from '../../shared/models/event.model';
import { EventType } from '../../shared/models/event.model';

const STORAGE_KEY = 'eventhub.eventList.filters';

function isEventType(value: unknown): value is EventType {
  return typeof value === 'string' && (Object.values(EventType) as string[]).includes(value);
}

function loadFromSession(): EventListFilterCriteria {
  if (typeof sessionStorage === 'undefined') {
    return { type: null, userId: '', createdFrom: '', createdTo: '' };
  }
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return { type: null, userId: '', createdFrom: '', createdTo: '' };
    }
    const parsed = JSON.parse(raw) as {
      type?: unknown;
      userId?: unknown;
      createdFrom?: unknown;
      createdTo?: unknown;
    };
    const type = parsed.type === null || parsed.type === undefined ? null : parsed.type;
    const userId = typeof parsed.userId === 'string' ? parsed.userId : '';
    const createdFrom = typeof parsed.createdFrom === 'string' ? parsed.createdFrom : '';
    const createdTo = typeof parsed.createdTo === 'string' ? parsed.createdTo : '';
    return {
      type: type !== null && isEventType(type) ? type : null,
      userId,
      createdFrom,
      createdTo,
    };
  } catch {
    return { type: null, userId: '', createdFrom: '', createdTo: '' };
  }
}

function persist(state: EventListFilterCriteria): void {
  if (typeof sessionStorage === 'undefined') {
    return;
  }
  try {
    sessionStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({
        type: state.type,
        userId: state.userId,
        createdFrom: state.createdFrom,
        createdTo: state.createdTo,
      }),
    );
  } catch {
    /* ignore quota / private mode */
  }
}

@Injectable({ providedIn: 'root' })
export class EventListFilterStateService {
  private readonly subject = new BehaviorSubject<EventListFilterCriteria>(loadFromSession());

  readonly state$ = this.subject.asObservable();

  get snapshot(): EventListFilterCriteria {
    return this.subject.value;
  }

  update(partial: Partial<EventListFilterCriteria>): void {
    const next: EventListFilterCriteria = {
      type: partial.type !== undefined ? partial.type : this.subject.value.type,
      userId: partial.userId !== undefined ? partial.userId : this.subject.value.userId,
      createdFrom:
        partial.createdFrom !== undefined ? partial.createdFrom : this.subject.value.createdFrom,
      createdTo: partial.createdTo !== undefined ? partial.createdTo : this.subject.value.createdTo,
    };
    const cur = this.subject.value;
    if (
      cur.type === next.type &&
      cur.userId === next.userId &&
      cur.createdFrom === next.createdFrom &&
      cur.createdTo === next.createdTo
    ) {
      return;
    }
    this.subject.next(next);
    persist(next);
  }
}
