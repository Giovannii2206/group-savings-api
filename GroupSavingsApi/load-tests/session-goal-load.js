import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  vus: 50,
  duration: '30s',
};

const BASE_URL = 'http://localhost:5000';

export default function () {
  // GET all group sessions
  let res = http.get(`${BASE_URL}/api/groupsession`);
  check(res, { 'group session status 200': (r) => r.status === 200 });

  // GET all savings goals
  let goalRes = http.get(`${BASE_URL}/api/savingsgoal`);
  check(goalRes, { 'savings goal status 200': (r) => r.status === 200 });

  sleep(1);
}
