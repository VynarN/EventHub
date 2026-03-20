# NFR verification: scalability and reliability

This document maps EventHub to **NFR1** (throughput / retrieval), **NFR2** (at-least-once delivery), **NFR3** (API latency), **NFR4** (UI grid load), and ties them to **Application Insights** validation. It complements the scaling decisions in `_bmad-output/planning-artifacts/architecture.md` (Scaling Strategy).

## 1. Architecture alignment (Azure staging / production)

| Area | Planned capability (architecture) | What to configure in Azure |
|------|-----------------------------------|----------------------------|
| Angular SPA | Static Web Apps + CDN | Use SWA; CDN is implicit. Scale is edge-cached static assets + API calls to backend. |
| .NET Web API | App Service autoscale | Enable scale-out rules on CPU, memory, or HTTP queue length; minimum instances sized for baseline. |
| Function App | Consumption plan autoscale | Keep queue-triggered scaling; ensure `ServiceBusConnection` and app settings are production-grade. |
| Service Bus | Premium for throughput SLAs | Use **Premium** when you need predictable throughput for **~1,000 msgs/s** ingress; tune messaging units. Standard tier is acceptable for lower environments with reduced targets. |
| Cosmos DB | Autoscale RU/s, partition key `Id` | Events container: autoscale throughput; monitor hot partitions via metrics. |

Local **docker-compose** uses emulators and is **not** sized to prove production NFR numbers; use it for functional smoke only.

## 2. At-least-once delivery (NFR2)

- **Service Bus → Function:** The isolated worker `ServiceBusTrigger` uses **peek-lock** semantics: the message is completed only after the function finishes successfully. If the function throws (e.g. Cosmos write failure), the message is **not** completed and will be **delivered again**.
- **Idempotent write:** `EventConsumerFunction` persists with `CosmosEventWriter.UpsertAsync` keyed by event `Id`. Redelivery of the same message overwrites the same logical document, avoiding duplicate rows for the same event id.

See `EventHub.FunctionApp/EventConsumerFunction.cs` and `EventHub.Cosmos/CosmosEventWriter.cs`.

## 3. Performance targets and how to check them

| NFR | Target | Primary check |
|-----|--------|----------------|
| NFR1 ingestion | Up to **1,000 events/s** sustained (staging) | k6 `ingest_nfr.js` with `NFR_STRICT=true` and tuned `ramping-arrival-rate` against a suitably scaled environment. |
| NFR1 retrieval | **100,000** events readable within **5 s** (aggregate read path) | k6 `list_events_nfr.js` with `NFR_STRICT=true`: parallel `GET /api/events?pageSize=100`; tune VUs and seed data in staging (see `load-tests/README.md`). |
| NFR3 API | **p95 &lt; 200 ms** for POST/GET under normal load | k6 thresholds on `http_req_duration` with `NFR_STRICT=true`. |
| NFR4 UI | Event grid **p95 &lt; 2 s** | Measure in browser (Lighthouse, Web Vitals) or E2E timing against staging; not covered by API k6 scripts alone. |

## 4. Application Insights (NFR6 / NFR7)

With `APPLICATIONINSIGHTS_CONNECTION_STRING` set on Web API and Function App:

- **Requests / dependencies:** Confirm POST/GET server timings and outbound calls (Cosmos, Service Bus).
- **Exceptions:** Failed invocations and API errors should appear linked to operation ids.
- **Custom traces:** Existing `ILogger` information logs (e.g. event created, listed, consumed) appear as traces.

Use portal **Logs** to validate p95 server duration vs. NFR3, e.g.:

```kusto
requests
| where name has "POST" or name has "GET"
| summarize p95=percentile(duration, 95) by bin(timestamp, 5m), name
```

## 5. Running load tests locally (smoke)

See `load-tests/README.md`. Smoke mode uses relaxed thresholds so a developer laptop + docker-compose can pass without proving production scale.

## 6. Definition of “verified” for this story

- **Repository:** k6 scripts + this document + README describe how to reproduce checks.
- **Staging:** Run strict k6 profiles against an environment that matches the scaling table above; confirm App Insights p95 and throughput goals.
- **Local:** Run smoke k6 (or skip if stack is down); goal is **script correctness**, not meeting 1,000 RPS on emulators.
