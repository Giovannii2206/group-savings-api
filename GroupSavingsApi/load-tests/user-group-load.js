import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 50, // Number of virtual users
  duration: '30s',
};

const BASE_URL = 'http://localhost:5000';

export default function () {
  // Example: GET all users
  let res = http.get(`${BASE_URL}/api/user`);
  check(res, { 'status was 200': (r) => r.status === 200 });

  // Example: Create a user
  let payload = JSON.stringify({
    username: `testuser${__VU}_${__ITER}`,
    password: 'testpass123',
    email: `test${__VU}_${__ITER}@example.com`
  });
  let headers = { 'Content-Type': 'application/json' };
  let createRes = http.post(`${BASE_URL}/api/user`, payload, { headers });
  check(createRes, { 'user created': (r) => r.status === 201 || r.status === 200 });

  // Example: GET all groups
  let groupRes = http.get(`${BASE_URL}/api/group`);
  check(groupRes, { 'groups status 200': (r) => r.status === 200 });

  sleep(1);
}
