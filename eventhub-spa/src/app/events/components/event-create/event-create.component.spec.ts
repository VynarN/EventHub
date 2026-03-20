import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { API_BASE_URL } from '../../../core/api-base-url.token';
import { EventType } from '../../../shared/models/event.model';
import { EventCreateComponent } from './event-create.component';

describe('EventCreateComponent', () => {
  let fixture: ComponentFixture<EventCreateComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EventCreateComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '' },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EventCreateComponent);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should POST EventCreationDto to /api/events on valid submit', () => {
    fixture.componentInstance.form.setValue({
      userId: 'user-1',
      type: EventType.Click,
      description: 'cta',
    });
    fixture.componentInstance.onSubmit();

    const req = httpMock.expectOne('/api/events');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      userId: 'user-1',
      type: 'click',
      description: 'cta',
    });
    req.flush({ id: 'x', userId: 'user-1', type: 'click', description: 'cta', createdAt: '2026-01-01T00:00:00Z' });
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.banner--success')?.textContent).toContain('Event created successfully');
    expect(fixture.componentInstance.form.value).toEqual({
      userId: '',
      type: null,
      description: '',
    });
  });

  it('should not call HTTP when form is invalid', () => {
    fixture.componentInstance.form.setValue({
      userId: '',
      type: null,
      description: '',
    });
    fixture.componentInstance.onSubmit();
    httpMock.expectNone('/api/events');
  });

  it('should show error banner on HTTP failure', () => {
    fixture.componentInstance.form.setValue({
      userId: 'u',
      type: EventType.PageView,
      description: '',
    });
    fixture.componentInstance.onSubmit();

    const req = httpMock.expectOne('/api/events');
    req.flush(
      { title: 'Invalid', errors: { userId: ['Too short'] } },
      { status: 400, statusText: 'Bad Request' },
    );
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.banner--error')?.textContent).toContain('Too short');
  });
});
