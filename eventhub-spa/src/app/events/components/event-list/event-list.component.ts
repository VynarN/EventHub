import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  BehaviorSubject,
  Subscription,
  combineLatest,
  distinctUntilChanged,
  map,
  switchMap,
} from 'rxjs';

import { EventFilterComponent } from '../event-filter/event-filter.component';
import { EventListFilterStateService } from '../../services/event-list-filter-state.service';
import { EventService } from '../../services/event.service';
import type { Event } from '../../../shared/models/event.model';

/** Default page size for the event data grid (server still enforces limits). */
export const DEFAULT_EVENT_LIST_PAGE_SIZE = 20;

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, EventFilterComponent],
  templateUrl: './event-list.component.html',
  styleUrl: './event-list.component.scss',
})
export class EventListComponent implements OnInit, OnDestroy {
  private readonly eventService = inject(EventService);
  private readonly filterState = inject(EventListFilterStateService);
  private sub?: Subscription;

  protected readonly items = signal<Event[]>([]);
  protected readonly loading = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly totalCount = signal(0);
  protected readonly pageSummary = signal<string | null>(null);

  private readonly pageNumber$ = new BehaviorSubject(1);
  private readonly pageSize$ = new BehaviorSubject(DEFAULT_EVENT_LIST_PAGE_SIZE);
  protected readonly pageNumber = toSignal(this.pageNumber$, { initialValue: 1 });
  protected readonly pageSize = toSignal(this.pageSize$, { initialValue: DEFAULT_EVENT_LIST_PAGE_SIZE });

  protected readonly totalPages = computed(() => {
    const tc = this.totalCount();
    const ps = this.pageSize();
    if (ps <= 0) {
      return 1;
    }
    return Math.max(1, Math.ceil(tc / ps));
  });

  protected readonly canGoPrev = computed(() => this.pageNumber() > 1);
  protected readonly canGoNext = computed(() => this.pageNumber() < this.totalPages());

  protected readonly pageSizeOptions = [10, 20, 50] as const;

  ngOnInit(): void {
    this.sub = this.filterState.state$
      .pipe(
        switchMap((criteria) => {
          // Reset page only after the previous inner subscription is torn down,
          // so the old combineLatest cannot react to this stream and duplicate GETs.
          if (this.pageNumber$.value !== 1) {
            this.pageNumber$.next(1);
          }
          return combineLatest([this.pageNumber$, this.pageSize$]).pipe(
            map(([pageNumber, pageSize]) => ({
              pageNumber,
              pageSize,
              type: criteria.type,
              userId: criteria.userId,
              createdFrom: criteria.createdFrom,
              createdTo: criteria.createdTo,
            })),
            distinctUntilChanged(
              (a, b) =>
                a.pageNumber === b.pageNumber &&
                a.pageSize === b.pageSize &&
                a.type === b.type &&
                a.userId === b.userId &&
                a.createdFrom === b.createdFrom &&
                a.createdTo === b.createdTo,
            ),
            switchMap(({ pageNumber, pageSize, type, userId, createdFrom, createdTo }) => {
              this.loading.set(true);
              this.loadError.set(null);
              return this.eventService.listEvents({
                pageNumber,
                pageSize,
                type,
                userId,
                createdFrom,
                createdTo,
              });
            }),
          );
        }),
      )
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          this.items.set(res.items);
          this.totalCount.set(res.totalCount);
          const pageCount =
            res.pageSize > 0 ? Math.max(1, Math.ceil(res.totalCount / res.pageSize)) : 1;
          this.pageSummary.set(
            res.totalCount === 0
              ? 'No events match the current filters.'
              : `Showing ${res.items.length} of ${res.totalCount} (page ${res.pageNumber} of ${pageCount}).`,
          );
        },
        error: (err: unknown) => {
          this.loading.set(false);
          this.items.set([]);
          this.totalCount.set(0);
          this.pageSummary.set(null);
          this.loadError.set(formatListError(err));
        },
      });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  protected goPrev(): void {
    if (this.canGoPrev()) {
      this.pageNumber$.next(this.pageNumber$.value - 1);
    }
  }

  protected goNext(): void {
    if (this.canGoNext()) {
      this.pageNumber$.next(this.pageNumber$.value + 1);
    }
  }

  protected goFirst(): void {
    this.pageNumber$.next(1);
  }

  protected goLast(): void {
    this.pageNumber$.next(this.totalPages());
  }

  protected onPageSizeSelect(value: string): void {
    const n = Number(value);
    if (!Number.isFinite(n) || n <= 0) {
      return;
    }
    this.pageNumber$.next(1);
    this.pageSize$.next(n);
  }
}

function formatListError(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) {
    return 'Unable to load events.';
  }
  if (err.status === 0) {
    return 'Unable to reach the server. Check that the API is running.';
  }
  return err.message || 'Request failed.';
}
