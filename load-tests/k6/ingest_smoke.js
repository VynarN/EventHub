import http from 'k6/http';
import { check } from 'k6';

const base = __ENV.BASE_URL || 'http://localhost:5080';
const strict = __ENV.NFR_STRICT === 'true';

export const options = {
  scenarios: {
    ingest: {
      executor: 'ramping-arrival-rate',
      startRate: 5,
      timeUnit: '1s',
      preAllocatedVUs: 20,
      maxVUs: 80,
      stages: [
        { duration: '15s', target: 20 },
        { duration: '45s', target: 40 },
        { duration: '15s', target: 0 },
      ],
    },
  },
  thresholds: strict
    ? {
        http_req_failed: ['rate<0.01'],
        'http_req_duration{name:post}': ['p(95)<200'],
      }
    : {
        http_req_failed: ['rate<0.05'],
        'http_req_duration{name:post}': ['p(95)<2000'],
      },
};

export default function () {
  const payload = JSON.stringify({
    userId: `k6-${__VU}-${__ITER}`,
    type: 'PageView',
    description: 'k6 smoke ingest',
  });
  const res = http.post(`${base}/api/events`, payload, {
    tags: { name: 'post' },
    headers: { 'Content-Type': 'application/json' },
  });
  check(res, { '201 Created': (r) => r.status === 201 });
}
