import http from 'k6/http';
import { check } from 'k6';

// Staging-oriented profile for NFR1 ingestion (~1000 events/s). Tune stages/VUs for your SKU.
const base = __ENV.BASE_URL || 'http://localhost:5080';
const strict = __ENV.NFR_STRICT === 'true';

export const options = {
  scenarios: {
    ingest: {
      executor: 'ramping-arrival-rate',
      startRate: strict ? 100 : 20,
      timeUnit: '1s',
      preAllocatedVUs: strict ? 400 : 50,
      maxVUs: strict ? 2000 : 100,
      stages: strict
        ? [
            { duration: '30s', target: 400 },
            { duration: '2m', target: 1000 },
            { duration: '30s', target: 0 },
          ]
        : [
            { duration: '20s', target: 50 },
            { duration: '1m', target: 100 },
            { duration: '20s', target: 0 },
          ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{name:post}': [strict ? 'p(95)<200' : 'p(95)<800'],
  },
};

export default function () {
  const payload = JSON.stringify({
    userId: `k6-nfr-${__VU}-${__ITER}`,
    type: 'Click',
    description: 'k6 nfr ingest',
  });
  const res = http.post(`${base}/api/events`, payload, {
    tags: { name: 'post' },
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, { '201 Created': (r) => r.status === 201 });
}
