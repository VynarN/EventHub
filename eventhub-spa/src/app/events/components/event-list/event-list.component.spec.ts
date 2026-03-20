import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../../core/api-base-url.token';
import { EventType } from '../../../shared/models/event.model';
import { EventListFilterStateService } from '../../services/event-list-filter-state.service';
import { EventService } from '../../services/event.service';
import { DEFAULT_EVENT_LIST_PAGE_SIZE, EventListComponent } from './event-list.component';

describe('EventListComponent', () => {
  let fixture: ComponentFixture<EventListComponent>;
  let httpMock: HttpTestingController;
  let filterState: EventListFilterStateService;

  beforeEach(async () => {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [EventListComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        EventService,
        EventListFilterStateService,
        { provide: API_BASE_URL, useValue: '' },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EventListComponent);
    httpMock = TestBed.inject(HttpTestingController);
    filterState = TestBed.inject(EventListFilterStateService);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
    TestBed.resetTestingModule();
  });

  it('should create', () => {
    fixture.detectChanges();
    const req = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(req.request.params.get('pageNumber')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe(String(DEFAULT_EVENT_LIST_PAGE_SIZE));
    req.flush({
      items: [],
      pageNumber: 1,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 0,
    });
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should refetch when filter state changes', () => {
    fixture.detectChanges();
    const first = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(first.request.params.get('userId')).toBeNull();
    first.flush({
      items: [
        {
          id: '1',
          userId: 'a',
          type: EventType.Click,
          description: 'd',
          createdAt: '2026-01-01T00:00:00Z',
        },
      ],
      pageNumber: 1,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 1,
    });
    fixture.detectChanges();
    const host = fixture.nativeElement as HTMLElement;
    expect(host.querySelectorAll('tbody tr').length).toBe(1);

    filterState.update({ userId: 'bob' });
    const second = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(second.request.params.get('userId')).toBe('bob');
    second.flush({
      items: [],
      pageNumber: 1,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 0,
    });
    fixture.detectChanges();
    expect(host.querySelectorAll('tbody tr').length).toBe(1);
    expect(host.textContent).toContain('No rows to display');
  });

  it('should request the next page when Next is clicked', async () => {
    fixture.detectChanges();
    const first = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    first.flush({
      items: Array.from({ length: DEFAULT_EVENT_LIST_PAGE_SIZE }, (_, i) => ({
        id: `id-${i}`,
        userId: 'u',
        type: EventType.Click,
        description: 'd',
        createdAt: '2026-01-01T00:00:00Z',
      })),
      pageNumber: 1,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 45,
    });
    fixture.detectChanges();

    const next = fixture.nativeElement.querySelector(
      '[aria-label="Next page"]',
    ) as HTMLButtonElement;
    expect(next.disabled).toBe(false);
    next.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const second = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(second.request.params.get('pageNumber')).toBe('2');
    second.flush({
      items: [],
      pageNumber: 2,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 45,
    });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Page 2 of 3');
  });

  it('should reset to page 1 when filters change after paging', async () => {
    fixture.detectChanges();
    const first = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    first.flush({
      items: Array.from({ length: DEFAULT_EVENT_LIST_PAGE_SIZE }, (_, i) => ({
        id: `id-${i}`,
        userId: 'u',
        type: EventType.Click,
        description: 'd',
        createdAt: '2026-01-01T00:00:00Z',
      })),
      pageNumber: 1,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 45,
    });
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('[aria-label="Next page"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    await fixture.whenStable();

    const page2 = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(page2.request.params.get('pageNumber')).toBe('2');
    page2.flush({
      items: [],
      pageNumber: 2,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 45,
    });
    fixture.detectChanges();

    filterState.update({ userId: 'reset-me' });
    fixture.detectChanges();
    await fixture.whenStable();

    const afterFilter = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(afterFilter.request.params.get('pageNumber')).toBe('1');
    expect(afterFilter.request.params.get('userId')).toBe('reset-me');
    afterFilter.flush({
      items: [],
      pageNumber: 1,
      pageSize: DEFAULT_EVENT_LIST_PAGE_SIZE,
      totalCount: 0,
    });
  });
});
