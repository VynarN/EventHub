import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../../core/api-base-url.token';
import type { EventListFilterCriteria, PagedEventsResponse } from '../../shared/models/event.model';

export interface ListEventsQuery extends EventListFilterCriteria {
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class EventService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);

  listEvents(query: ListEventsQuery): Observable<PagedEventsResponse> {
    const pageNumber = query.pageNumber ?? 1;
    const pageSize = query.pageSize ?? 50;
    let params = new HttpParams()
      .set('pageNumber', String(pageNumber))
      .set('pageSize', String(pageSize));

    if (query.type != null) {
      params = params.set('type', query.type);
    }
    const userId = (query.userId ?? '').trim();
    if (userId.length > 0) {
      params = params.set('userId', userId);
    }
    const createdFrom = (query.createdFrom ?? '').trim();
    if (createdFrom.length > 0) {
      params = params.set('createdFrom', createdFrom);
    }
    const createdTo = (query.createdTo ?? '').trim();
    if (createdTo.length > 0) {
      params = params.set('createdTo', createdTo);
    }

    const base = this.apiBaseUrl.replace(/\/$/, '');
    const url = base ? `${base}/api/events` : '/api/events';
    return this.http.get<PagedEventsResponse>(url, { params });
  }
}
