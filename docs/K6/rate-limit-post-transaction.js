import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

// métricas customizadas
const status200 = new Counter('status_200');
const status429 = new Counter('status_429');
const statusOther = new Counter('status_other');

export let options = {
   scenarios: {
     rate_test: {
       executor: 'constant-arrival-rate',
       rate: 100,        // força carga
       timeUnit: '1s',
       duration: '20s',
       preAllocatedVUs: 100,
       maxVUs: 200,
     },
   

    // ramp_test: {
    //   executor: 'ramping-arrival-rate',
    //   startRate: 10, // começa leve
    //   timeUnit: '1s',
    //   preAllocatedVUs: 50,
    //   maxVUs: 200,
    //   stages: [
    //     { target: 20, duration: '10s' },
    //     { target: 40, duration: '10s' },
    //     { target: 60, duration: '10s' },
    //     { target: 80, duration: '10s' },
    //     { target: 100, duration: '10s' },
    //   ],
    // },
  },
  insecureSkipTLSVerify: true,
};

export default function () {
  const payload = JSON.stringify({
    amount: 100.0,
    type: "Credit"
  });

  const headers = {
    'Content-Type': 'application/json',
    'X-Tenant-Id': '11111116',
    'X-Idempotence-Key': `req-${Date.now()}-${Math.random()}`,
    'Authorization': 'Bearer token'
  };

  const res = http.post('https://localhost:7165/transactions', payload, { headers });

  // métricas
  if (res.status === 200 || res.status === 201) {
    status200.add(1);
  } else if (res.status === 429) {
    status429.add(1);
  } else {
    statusOther.add(1);
  }

  // validação básica
  check(res, {
    'status esperado': (r) =>
      [200, 201, 400, 401, 429].includes(r.status),
  });
}