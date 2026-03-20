import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Subscription, debounceTime, distinctUntilChanged, map } from 'rxjs';

import { EventListFilterStateService } from '../../services/event-list-filter-state.service';
import { EventType } from '../../../shared/models/event.model';
import { datetimeLocalToIsoUtc, isoUtcToDatetimeLocal } from '../../utils/datetime-local';

@Component({
  selector: 'app-event-filter',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './event-filter.component.html',
  styleUrl: './event-filter.component.scss',
})
export class EventFilterComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly filterState = inject(EventListFilterStateService);
  private sub?: Subscription;

  protected readonly EventType = EventType;
  protected readonly typeOptions: { value: EventType | null; label: string }[] = [
    { value: null, label: 'Any type' },
    { value: EventType.PageView, label: 'Page view' },
    { value: EventType.Click, label: 'Click' },
    { value: EventType.Purchase, label: 'Purchase' },
  ];

  readonly form = this.fb.group({
    type: this.fb.control<EventType | null>(null),
    userId: [''],
    createdFrom: [''],
    createdTo: [''],
  });

  ngOnInit(): void {
    const s = this.filterState.snapshot;
    this.form.patchValue(
      {
        type: s.type,
        userId: s.userId,
        createdFrom: isoUtcToDatetimeLocal(s.createdFrom),
        createdTo: isoUtcToDatetimeLocal(s.createdTo),
      },
      { emitEvent: false },
    );

    const stateSync = this.filterState.state$.subscribe((next) => {
      this.form.patchValue(
        {
          type: next.type,
          userId: next.userId,
          createdFrom: isoUtcToDatetimeLocal(next.createdFrom),
          createdTo: isoUtcToDatetimeLocal(next.createdTo),
        },
        { emitEvent: false },
      );
    });

    const valueSub = this.form.valueChanges
      .pipe(
        debounceTime(300),
        map((v) => ({
          type: v.type ?? null,
          userId: (v.userId ?? '').trim(),
          createdFrom: datetimeLocalToIsoUtc(v.createdFrom ?? ''),
          createdTo: datetimeLocalToIsoUtc(v.createdTo ?? ''),
        })),
        distinctUntilChanged(
          (a, b) =>
            a.type === b.type &&
            a.userId === b.userId &&
            a.createdFrom === b.createdFrom &&
            a.createdTo === b.createdTo,
        ),
      )
      .subscribe((criteria) => this.filterState.update(criteria));

    this.sub = new Subscription();
    this.sub.add(stateSync);
    this.sub.add(valueSub);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
