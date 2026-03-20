import { InjectionToken } from '@angular/core';

/** Browser API origin when SPA is served on port 4200 (local or Docker); empty otherwise for same-origin / proxy. */
export function apiBaseUrlFactory(): string {
  if (typeof window === 'undefined' || !window.location?.port) {
    return '';
  }
  if (window.location.port === '4200') {
    const { protocol, hostname } = window.location;
    return `${protocol}//${hostname}:5080`;
  }
  return '';
}

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  factory: apiBaseUrlFactory,
});
