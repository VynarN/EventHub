/** `datetime-local` value → ISO 8601 UTC for the API, or empty when unset/invalid. */
export function datetimeLocalToIsoUtc(local: string): string {
  const t = local.trim();
  if (!t) {
    return '';
  }
  const d = new Date(t);
  return Number.isNaN(d.getTime()) ? '' : d.toISOString();
}

/** ISO 8601 UTC → `datetime-local` value in the user's local timezone. */
export function isoUtcToDatetimeLocal(iso: string): string {
  const t = iso.trim();
  if (!t) {
    return '';
  }
  const d = new Date(t);
  if (Number.isNaN(d.getTime())) {
    return '';
  }
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
