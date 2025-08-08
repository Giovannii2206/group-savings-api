import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 50,
  duration: '30s',
};

const BASE_URL = 'http://localhost:5000';

export default function () {
  // GET all notifications
  let res = http.get(`${BASE_URL}/api/notification`);
  check(res, { 'notification status 200': (r) => r.status === 200 });

  // GET all audit logs
  let auditRes = http.get(`${BASE_URL}/api/auditlog`);
  check(auditRes, { 'audit log status 200': (r) => r.status === 200 });

  sleep(1);
}
