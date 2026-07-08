import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 15,
  duration: '2m',
  thresholds: {
    http_req_failed: ['rate<0.01'], // less than 1% errors
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
  },
};

const BASE_URL = __ENV.API_URL || 'https://localhost:5001/api/v1';

export function setup() {
  const email = __ENV.TEST_EMAIL;
  const password = __ENV.TEST_PASSWORD;

  if (!email || !password) {
    throw new Error('TEST_EMAIL and TEST_PASSWORD environment variables are required.');
  }

  const loginRes = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    email: email,
    password: password
  }), {
    headers: { 'Content-Type': 'application/json' }
  });

  if (loginRes.status !== 200) {
    throw new Error(`Login failed with status ${loginRes.status}: ${loginRes.body}`);
  }

  const authData = loginRes.json();
  return { token: authData.accessToken };
}

export default function (data) {
  const headers = {
    'Authorization': `Bearer ${data.token}`,
    'Content-Type': 'application/json'
  };

  const rnd = Math.random();

  if (rnd < 0.70) {
    // 70% Reads: List endpoints
    const endpoints = [
      '/fleets',
      '/ships',
      '/voyages',
      '/notifications',
      '/analytics/fleet-summary'
    ];
    const endpoint = endpoints[Math.floor(Math.random() * endpoints.length)];
    const res = http.get(`${BASE_URL}${endpoint}?pageSize=100`, { headers });
    check(res, { 'list endpoint returns 200': (r) => r.status === 200 });
  } 
  else if (rnd < 0.90) {
    // 20% Reads: Detail endpoints
    // Note: To avoid hardcoding a seeded ID, we fetch the first page of ships and pick one.
    // In a pure high-perf test we'd pass IDs from setup(), but this adds realistic client behavior.
    const shipsRes = http.get(`${BASE_URL}/ships?pageSize=5`, { headers });
    if (shipsRes.status === 200) {
      const ships = shipsRes.json('items');
      if (ships && ships.length > 0) {
        const shipId = ships[0].id;
        const detailRes = http.get(`${BASE_URL}/ships/${shipId}`, { headers });
        check(detailRes, { 'detail endpoint returns 200': (r) => r.status === 200 });
      }
    }
  } 
  else {
    // 10% Writes: Create an incident
    // Note: We need a ship ID to create an incident for.
    const shipsRes = http.get(`${BASE_URL}/ships?pageSize=1`, { headers });
    if (shipsRes.status === 200) {
      const ships = shipsRes.json('items');
      if (ships && ships.length > 0) {
        const shipId = ships[0].id;
        
        const payload = JSON.stringify({
          title: `Load Test Incident ${Math.floor(Math.random() * 10000)}`,
          description: 'This is a simulated incident generated during the load test baseline.',
          severity: 'Low',
          status: 'Open',
          shipId: shipId,
          occurredAt: new Date().toISOString()
        });

        const postRes = http.post(`${BASE_URL}/incidents`, payload, { headers });
        check(postRes, { 'create incident returns 201': (r) => r.status === 201 });
      }
    }
  }

  // Brief pause to simulate realistic user pacing
  sleep(1);
}
