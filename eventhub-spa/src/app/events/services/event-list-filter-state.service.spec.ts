import { filter, firstValueFrom, take } from 'rxjs';

import { EventType } from '../../shared/models/event.model';
import { EventListFilterStateService } from './event-list-filter-state.service';

describe('EventListFilterStateService', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should load persisted state from sessionStorage', () => {
    sessionStorage.setItem(
      'eventhub.eventList.filters',
      JSON.stringify({ type: EventType.PageView, userId: 'u1' }),
    );
    const service = new EventListFilterStateService();
    expect(service.snapshot).toEqual({
      type: EventType.PageView,
      userId: 'u1',
      createdFrom: '',
      createdTo: '',
    });
  });

  it('should update snapshot and persist on update()', () => {
    const service = new EventListFilterStateService();
    service.update({ type: EventType.Purchase, userId: 'buyer' });
    expect(service.snapshot).toEqual({
      type: EventType.Purchase,
      userId: 'buyer',
      createdFrom: '',
      createdTo: '',
    });
    const raw = sessionStorage.getItem('eventhub.eventList.filters');
    expect(raw).toBeTruthy();
    const parsed = JSON.parse(raw!);
    expect(parsed).toEqual({
      type: EventType.Purchase,
      userId: 'buyer',
      createdFrom: '',
      createdTo: '',
    });
  });

  it('should emit on state$ when update is called', async () => {
    const service = new EventListFilterStateService();
    const pending = firstValueFrom(
      service.state$.pipe(
        filter((s) => s.userId === 'x'),
        take(1),
      ),
    );
    service.update({ userId: 'x' });
    const s = await pending;
    expect(s.type).toBeNull();
    expect(s.userId).toBe('x');
  });
});
