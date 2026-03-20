import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

// Read-path stress (NFR1 retrieval). For "100k in 5s", run against staging with enough data and tune VUs/duration.
const base = __ENV.BASE_URL || 'http://localhost:5080';
const strict = __ENV.NFR_STRICT === 'true';
const itemsFetched = new Counter('items_fetched');

export const options = {
  scenarios: {
    list: {
      executor: 'ramping-vus',
      startVUs: strict ? 50 : 5,
      stages: strict
        ? [
            { duration: '5s', target: 500 },
            { duration: '5s', target: 1000 },
            { duration: '5s', target: 0 },
          ]
        : [
            { duration: '20s', target: 20 },
            { duration: '1m', target: 40 },
            { duration: '20s', target: 0 },
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
        'http_req_duration{name:get}': ['p(95)<1500'],
      },
};

export default function () {
  const page = (__ITER % 50) + 1;
  const res = http.get(`${base}/api/events?pageNumber=${page}&pageSize=100`, {
    tags: { name: 'get' },
  });
  const requestOk = check(res, { '200 OK': (r) => r.status === 200 });
  if (requestOk) {
    try {
      const body = res.json();
      if (body && Array.isArray(body.items)) {
        itemsFetched.add(body.items.length);
      }
    } catch {
      // ignore
    }
  }
}
