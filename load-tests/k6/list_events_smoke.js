import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const base = __ENV.BASE_URL || 'http://localhost:5080';
const strict = __ENV.NFR_STRICT === 'true';
const itemsFetched = new Counter('items_fetched');

export const options = {
  scenarios: {
    list: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '15s', target: 10 },
        { duration: '45s', target: 25 },
        { duration: '15s', target: 0 },
      ],
    },
  },
  thresholds: strict
    ? {
        http_req_failed: ['rate<0.01'],
        'http_req_duration{name:get}': ['p(95)<200'],
      }
    : {
        http_req_failed: ['rate<0.05'],
        'http_req_duration{name:get}': ['p(95)<3000'],
      },
};

export default function () {
  const res = http.get(`${base}/api/events?pageNumber=1&pageSize=100`, {
    tags: { name: 'get' },
  });
  const ok = check(res, { '200 OK': (r) => r.status === 200 });
  if (ok) {
    try {
      const body = res.json();
      if (body && Array.isArray(body.items)) {
        itemsFetched.add(body.items.length);
      }
    } catch {
      // ignore parse errors in load test
    }
  }
}
