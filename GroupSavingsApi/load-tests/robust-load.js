import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 100, // High concurrency
  duration: '60s',
};

const BASE_URL = 'http://localhost:5000';

export default function () {
  // GET all members
  let res = http.get(`${BASE_URL}/api/member`);
  check(res, { 'member status 200': (r) => r.status === 200 });

  // GET all group members
  let groupRes = http.get(`${BASE_URL}/api/groupmember`);
  check(groupRes, { 'groupmember status 200': (r) => r.status === 200 });

  // POST: create a notification
  let notifPayload = JSON.stringify({
    message: `Load test notification ${__VU}_${__ITER}`,
    userId: 1
  });
  let headers = { 'Content-Type': 'application/json' };
  let notifRes = http.post(`${BASE_URL}/api/notification`, notifPayload, { headers });
  check(notifRes, { 'notification created': (r) => r.status === 201 || r.status === 200 });

  // GET: health check
  let health = http.get(`${BASE_URL}/api/health`);
  check(health, { 'health status 200': (r) => r.status === 200 });

  sleep(0.5);
}
