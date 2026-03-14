# API Gateway — Manual Test Cases

## Prerequisites
- All services running (`docker compose up`)
- Gateway available at `http://localhost:5000`
- Swagger UI at `http://localhost:5000/swagger/index.html`

---

## TC-GW-001: Health Check
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `GET http://localhost:5000/health` | 200 OK, `{"status":"healthy"}` |
| 2 | `GET http://localhost:5000/health/ready` | 200 OK |
| 3 | `GET http://localhost:5000/health/live` | 200 OK |

---

## TC-GW-002: Valid Event Submission
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST `/api/events` with `{"eventType":"USER_REGISTERED","userId":"u1","email":"u@e.com"}` | 2xx, response contains `id` |
| 2 | POST with `PAYMENT_FAILED` | 2xx |
| 3 | POST with `ORDER_SHIPPED` | 2xx |
| 4 | POST with `SECURITY_ALERT` | 2xx |

---

## TC-GW-003: Validation — Invalid Event Type
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST `/api/events` with `{"eventType":"INVALID","userId":"u1","email":"u@e.com"}` | 400, error mentions `eventType` |

---

## TC-GW-004: Validation — Missing Required Fields
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST with missing `userId` | 400 |
| 2 | POST with missing `email` | 400 |
| 3 | POST with empty body `{}` | 400 |
| 4 | POST with empty strings for all fields | 400 |

---

## TC-GW-005: Validation — Invalid Email
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST with `"email":"not-an-email"` | 400 |

---

## TC-GW-006: Swagger UI Accessibility
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open `http://localhost:5000/swagger/index.html` | Swagger UI loads |
| 2 | Verify POST /api/events is listed | Visible in the page |
| 3 | Click "Try it out" → Execute with valid payload | 2xx response |

---

## TC-GW-007: Prometheus Metrics
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `GET http://localhost:5000/metrics` | 200, Prometheus text format |
| 2 | Send a few requests, re-check `/metrics` | HTTP request counters increase |

---

## TC-GW-008: Circuit Breaker Behaviour
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Stop Event Service container | Container stopped |
| 2 | POST `/api/events` with valid payload (×5) | 502 or 503 errors |
| 3 | Continue sending requests | Circuit opens — fast failures |
| 4 | Restart Event Service container | Container running |
| 5 | Wait 30 seconds, POST again | Circuit half-open → success → circuit closes |

---

## TC-GW-009: Request Logging
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Send any request to gateway | Docker logs show structured JSON log entry |
| 2 | Verify log contains: method, path, status, elapsed | All fields present |

---

## TC-GW-010: Correlation ID
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST `/api/events` with valid payload | Response headers contain `X-Correlation-Id` or `X-Request-Id` |
| 2 | Check logs for the same correlation ID | Present in structured log |
