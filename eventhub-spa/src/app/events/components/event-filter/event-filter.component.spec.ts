import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventListFilterStateService } from '../../services/event-list-filter-state.service';
import { EventType } from '../../../shared/models/event.model';
import { EventFilterComponent } from './event-filter.component';

describe('EventFilterComponent', () => {
  let fixture: ComponentFixture<EventFilterComponent>;
  let filterState: EventListFilterStateService;

  beforeEach(async () => {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [EventFilterComponent],
      providers: [EventListFilterStateService],
    }).compileComponents();

    fixture = TestBed.createComponent(EventFilterComponent);
    filterState = TestBed.inject(EventListFilterStateService);
  });

  afterEach(() => {
    sessionStorage.clear();
    TestBed.resetTestingModule();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should hydrate form from persisted filter state', () => {
    filterState.update({ type: EventType.Click, userId: 'stored-user' });
    fixture.detectChanges();
    expect(fixture.componentInstance.form.getRawValue()).toEqual({
      type: EventType.Click,
      userId: 'stored-user',
      createdFrom: '',
      createdTo: '',
    });
  });

  it('should push debounced filter changes to EventListFilterStateService', async () => {
    fixture.detectChanges();
    filterState.update({ type: null, userId: '', createdFrom: '', createdTo: '' });

    fixture.componentInstance.form.patchValue({ userId: 'abc' });
    await new Promise((r) => setTimeout(r, 299));
    expect(filterState.snapshot.userId).toBe('');
    await new Promise((r) => setTimeout(r, 5));
    expect(filterState.snapshot.userId).toBe('abc');
  });
});
