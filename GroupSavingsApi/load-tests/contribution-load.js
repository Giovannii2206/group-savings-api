import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 50,
  duration: '30s',
};

const BASE_URL = 'http://localhost:5000';

export default function () {
  // Example: GET all contributions
  let res = http.get(`${BASE_URL}/api/contribution`);
  check(res, { 'contribution status 200': (r) => r.status === 200 });

  // Example: Create a contribution
  let payload = JSON.stringify({
    amount: Math.floor(Math.random() * 1000),
    groupSessionId: 1,
    memberId: 1,
    date: new Date().toISOString()
  });
  let headers = { 'Content-Type': 'application/json' };
  let createRes = http.post(`${BASE_URL}/api/contribution`, payload, { headers });
  check(createRes, { 'contribution created': (r) => r.status === 201 || r.status === 200 });

  sleep(1);
}
