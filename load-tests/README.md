# Load tests (k6)

Scripts exercise **POST /api/events** (ingestion) and **GET /api/events** (list / read path). They support two modes:

| Mode | Env | Use |
|------|-----|-----|
| **Smoke** (default) | _(omit `NFR_STRICT`)_ | Local docker-compose; relaxed p95 and RPS. |
| **Strict NFR** | `NFR_STRICT=true` | Staging / performance environment; enforces architecture NFR-oriented thresholds (tune stages if the environment differs). |

## Prerequisites

- Running Web API (e.g. `docker compose up` with `eventhub-webapi` on port **5080**).
- [k6](https://k6.io/docs/get-started/installation/) **or** Docker:

```powershell
# From repo root — k6 in Docker (Windows: reach host API)
docker run --rm -e BASE_URL=http://host.docker.internal:5080 -v "${PWD}/load-tests/k6:/scripts" grafana/k6 run /scripts/ingest_smoke.js
```

Adjust `BASE_URL` if the API is elsewhere (e.g. `http://localhost:5080` when k6 runs on the host).

## Scripts

- `k6/ingest_smoke.js` — ramping arrival rate for POST; smoke thresholds by default.
- `k6/ingest_nfr.js` — stricter arrival rate and **p95 &lt; 200ms** when `NFR_STRICT=true`.
- `k6/list_events_smoke.js` — parallel GET with `pageSize=100`.
- `k6/list_events_nfr.js` — higher parallelism for read stress when `NFR_STRICT=true`.

Examples:

```powershell
# Smoke (local)
$env:BASE_URL = "http://localhost:5080"
k6 run load-tests/k6/ingest_smoke.js
k6 run load-tests/k6/list_events_smoke.js

# Strict (staging) — tune `stages` / `preAllocatedVUs` to match capacity
$env:BASE_URL = "https://your-api.azurewebsites.net"
$env:NFR_STRICT = "true"
k6 run load-tests/k6/ingest_nfr.js
k6 run load-tests/k6/list_events_nfr.js
```

## NFR1 read path (100k in 5s)

The **list_events_nfr** script uses aggressive `ramping-vus` and a custom **`items_fetched`** counter (sum of `items.length` per response) for observability in the k6 summary. Meeting **100,000 events within 5 seconds** requires a suitably scaled staging environment, enough data, and tuned stages/VUs; interpret the counter and request latency together with Cosmos and App Insights metrics rather than relying on a single generic threshold.

## CI

Optional: add a job that starts the stack (or targets a deployed dev URL) and runs `ingest_smoke.js` only, failing the build on `http_req_failed`.
