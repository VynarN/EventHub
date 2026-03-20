import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { API_BASE_URL } from '../../../core/api-base-url.token';
import type { EventCreationDto } from '../../../shared/models/event-creation.dto';
import { EventType } from '../../../shared/models/event.model';

@Component({
  selector: 'app-event-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-create.component.html',
  styleUrl: './event-create.component.scss',
})
export class EventCreateComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  protected readonly submitting = signal(false);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly submitError = signal<string | null>(null);

  protected readonly EventType = EventType;
  protected readonly typeOptions: { value: EventType; label: string }[] = [
    { value: EventType.PageView, label: 'Page view' },
    { value: EventType.Click, label: 'Click' },
    { value: EventType.Purchase, label: 'Purchase' },
  ];

  readonly form = this.fb.group({
    userId: ['', [Validators.required, Validators.minLength(1)]],
    type: this.fb.control<EventType | null>(null, Validators.required),
    description: [''],
  });

  private eventsPostUrl(): string {
    const base = this.apiBaseUrl.replace(/\/$/, '');
    return base ? `${base}/api/events` : '/api/events';
  }

  onSubmit(): void {
    this.successMessage.set(null);
    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const userId = (raw.userId ?? '').trim();
    const payload: EventCreationDto = {
      userId,
      type: raw.type!,
      description: (raw.description ?? '').trim(),
    };

    this.submitting.set(true);
    this.http.post<unknown>(this.eventsPostUrl(), payload).subscribe({
      next: () => {
        this.submitting.set(false);
        this.successMessage.set('Event created successfully.');
        this.form.reset({ userId: '', type: null, description: '' });
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.submitError.set(formatHttpError(err));
      },
    });
  }
}

function formatHttpError(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }
  if (err.status === 0) {
    return 'Unable to reach the server. Check that the API is running.';
  }
  const body = err.error;
  if (body && typeof body === 'object') {
    const o = body as Record<string, unknown>;
    const errors = o['errors'];
    if (errors && typeof errors === 'object') {
      const messages: string[] = [];
      for (const value of Object.values(errors as Record<string, unknown>)) {
        if (Array.isArray(value)) {
          for (const m of value) {
            if (typeof m === 'string') {
              messages.push(m);
            }
          }
        }
      }
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }
    if (typeof o['detail'] === 'string' && o['detail'].length > 0) {
      return o['detail'];
    }
    if (typeof o['title'] === 'string' && o['title'].length > 0) {
      return o['title'];
    }
  }
  if (typeof err.error === 'string' && err.error.length > 0) {
    return err.error;
  }
  return err.message || 'Request failed. Please check your input and try again.';
}
