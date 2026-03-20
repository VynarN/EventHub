import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../core/api-base-url.token';
import { EventType } from '../../shared/models/event.model';
import { EventService } from './event.service';

describe('EventService', () => {
  let service: EventService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        EventService,
        { provide: API_BASE_URL, useValue: '' },
      ],
    });
    service = TestBed.inject(EventService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    TestBed.resetTestingModule();
  });

  it('should GET /api/events with page params only when no filters', () => {
    service
      .listEvents({
        pageNumber: 2,
        pageSize: 10,
        type: null,
        userId: '',
        createdFrom: '',
        createdTo: '',
      })
      .subscribe();

    const req = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('type')).toBeNull();
    expect(req.request.params.get('userId')).toBeNull();
    expect(req.request.params.get('createdFrom')).toBeNull();
    expect(req.request.params.get('createdTo')).toBeNull();
    req.flush({ items: [], pageNumber: 2, pageSize: 10, totalCount: 0 });
  });

  it('should include type and userId when set', () => {
    service
      .listEvents({
        type: EventType.Click,
        userId: '  alice  ',
        createdFrom: '',
        createdTo: '',
      })
      .subscribe();

    const req = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(req.request.params.get('type')).toBe('click');
    expect(req.request.params.get('userId')).toBe('alice');
    req.flush({ items: [], pageNumber: 1, pageSize: 50, totalCount: 0 });
  });

  it('should include createdFrom and createdTo when set', () => {
    service
      .listEvents({
        type: null,
        userId: '',
        createdFrom: '2026-01-10T12:00:00.000Z',
        createdTo: '2026-01-20T12:00:00.000Z',
      })
      .subscribe();

    const req = httpMock.expectOne((r) => r.url.startsWith('/api/events'));
    expect(req.request.params.get('createdFrom')).toBe('2026-01-10T12:00:00.000Z');
    expect(req.request.params.get('createdTo')).toBe('2026-01-20T12:00:00.000Z');
    req.flush({ items: [], pageNumber: 1, pageSize: 50, totalCount: 0 });
  });
});
