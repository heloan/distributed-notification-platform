# 🧪 Manual Test Cases — API Gateway

> Step-by-step manual test procedures for the API Gateway endpoints.
> Execute these tests against a running Gateway instance.

---

## Prerequisites

- API Gateway running on `http://localhost:5000`
- Tool: `curl`, Postman, or any HTTP client

---

## TC-001: Health Check — Liveness

| Field | Value |
|-------|-------|
| **ID** | TC-001 |
| **Title** | Health check returns healthy status |
| **Priority** | Critical |
| **Endpoint** | `GET /health` |

**Steps:**

1. Send `GET http://localhost:5000/health`

**Expected Result:**

- Status Code: `200 OK`
- Body contains:
  ```json
  {
    "status": "Healthy",
    "service": "API Gateway",
    "timestamp": "2026-03-14T..."
  }
  ```

---

## TC-002: Health Check — Readiness (Event Service Down)

| Field | Value |
|-------|-------|
| **ID** | TC-002 |
| **Title** | Readiness check reports unhealthy when Event Service is down |
| **Priority** | High |
| **Endpoint** | `GET /health/ready` |

**Steps:**

1. Ensure the Event Service (Java) is **not running**
2. Send `GET http://localhost:5000/health/ready`

**Expected Result:**

- Status Code: `503 Service Unavailable`
- Body contains `"status": "Unhealthy"`

---

## TC-003: Submit Event — Valid USER_REGISTERED

| Field | Value |
|-------|-------|
| **ID** | TC-003 |
| **Title** | Submit a valid USER_REGISTERED event |
| **Priority** | Critical |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send:
   ```bash
   curl -X POST http://localhost:5000/events \
     -H "Content-Type: application/json" \
     -d '{
       "eventType": "USER_REGISTERED",
       "userId": "123",
       "email": "user@email.com",
       "timestamp": "2026-03-14T10:00:00"
     }'
   ```

**Expected Result (Event Service running):**

- Status Code: `202 Accepted`
- Body contains event response with `id`, `eventType`, `status`

**Expected Result (Event Service NOT running):**

- Status Code: `502 Bad Gateway`
- Body:
  ```json
  {
    "title": "Bad Gateway",
    "status": 502,
    "detail": "The downstream service is currently unavailable. Please try again later."
  }
  ```

---

## TC-004: Submit Event — Valid PAYMENT_FAILED

| Field | Value |
|-------|-------|
| **ID** | TC-004 |
| **Title** | Submit a valid PAYMENT_FAILED event |
| **Priority** | High |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send:
   ```bash
   curl -X POST http://localhost:5000/events \
     -H "Content-Type: application/json" \
     -d '{
       "eventType": "PAYMENT_FAILED",
       "userId": "456",
       "email": "user@email.com"
     }'
   ```

**Expected Result:**

- Status Code: `202 Accepted` (if Event Service is running)
- Status Code: `502 Bad Gateway` (if Event Service is down)

---

## TC-005: Submit Event — Invalid Event Type

| Field | Value |
|-------|-------|
| **ID** | TC-005 |
| **Title** | Reject event with unsupported event type |
| **Priority** | High |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send:
   ```bash
   curl -X POST http://localhost:5000/events \
     -H "Content-Type: application/json" \
     -d '{
       "eventType": "INVALID_TYPE",
       "userId": "123",
       "email": "user@email.com"
     }'
   ```

**Expected Result:**

- Status Code: `400 Bad Request`
- Body contains validation error:
  ```json
  {
    "title": "Validation Failed",
    "status": 400,
    "detail": "One or more validation errors occurred.",
    "errors": {
      "EventType": ["EventType must be one of: USER_REGISTERED, PAYMENT_FAILED, ORDER_SHIPPED, SECURITY_ALERT."]
    }
  }
  ```

---

## TC-006: Submit Event — Missing Email

| Field | Value |
|-------|-------|
| **ID** | TC-006 |
| **Title** | Reject event with empty email |
| **Priority** | High |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send:
   ```bash
   curl -X POST http://localhost:5000/events \
     -H "Content-Type: application/json" \
     -d '{
       "eventType": "USER_REGISTERED",
       "userId": "123",
       "email": ""
     }'
   ```

**Expected Result:**

- Status Code: `400 Bad Request`
- Errors include `"Email"` field

---

## TC-007: Submit Event — Invalid Email Format

| Field | Value |
|-------|-------|
| **ID** | TC-007 |
| **Title** | Reject event with invalid email format |
| **Priority** | Medium |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send with `"email": "not-an-email"`

**Expected Result:**

- Status Code: `400 Bad Request`
- Error: `"Email must be a valid email address."`

---

## TC-008: Submit Event — Missing UserId

| Field | Value |
|-------|-------|
| **ID** | TC-008 |
| **Title** | Reject event with empty userId |
| **Priority** | High |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send with `"userId": ""`

**Expected Result:**

- Status Code: `400 Bad Request`
- Error: `"UserId is required."`

---

## TC-009: Submit Event — Future Timestamp

| Field | Value |
|-------|-------|
| **ID** | TC-009 |
| **Title** | Reject event with timestamp far in the future |
| **Priority** | Medium |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send with `"timestamp": "2030-01-01T00:00:00"`

**Expected Result:**

- Status Code: `400 Bad Request`
- Error: `"Timestamp cannot be in the future."`

---

## TC-010: Submit Event — Multiple Validation Errors

| Field | Value |
|-------|-------|
| **ID** | TC-010 |
| **Title** | Return all validation errors at once |
| **Priority** | Medium |
| **Endpoint** | `POST /events` |

**Steps:**

1. Send:
   ```bash
   curl -X POST http://localhost:5000/events \
     -H "Content-Type: application/json" \
     -d '{
       "eventType": "",
       "userId": "",
       "email": ""
     }'
   ```

**Expected Result:**

- Status Code: `400 Bad Request`
- Errors object contains keys for `EventType`, `UserId`, and `Email`

---

## TC-011: Get All Events

| Field | Value |
|-------|-------|
| **ID** | TC-011 |
| **Title** | Retrieve all events |
| **Priority** | High |
| **Endpoint** | `GET /events` |

**Steps:**

1. Send `GET http://localhost:5000/events`

**Expected Result:**

- Status Code: `200 OK` (Event Service running)
- Status Code: `502 Bad Gateway` (Event Service down)

---

## TC-012: Get Event by ID — Not Found

| Field | Value |
|-------|-------|
| **ID** | TC-012 |
| **Title** | Retrieve non-existent event by ID |
| **Priority** | Medium |
| **Endpoint** | `GET /events/{id}` |

**Steps:**

1. Send `GET http://localhost:5000/events/non-existent-id`

**Expected Result (Event Service running):**

- Status Code: `404 Not Found`

---

## TC-013: Swagger UI Accessible

| Field | Value |
|-------|-------|
| **ID** | TC-013 |
| **Title** | Swagger UI loads in Development mode |
| **Priority** | Medium |
| **Endpoint** | `GET /swagger` |

**Steps:**

1. Open `http://localhost:5000/swagger` in a browser

**Expected Result:**

- Swagger UI page loads
- Shows "DSNP — API Gateway" title
- Lists all endpoints: POST /events, GET /events, GET /events/{id}, GET /health

---

## TC-014: Prometheus Metrics Endpoint

| Field | Value |
|-------|-------|
| **ID** | TC-014 |
| **Title** | Prometheus metrics are exposed |
| **Priority** | Medium |
| **Endpoint** | `GET /metrics` |

**Steps:**

1. Send `GET http://localhost:5000/metrics`

**Expected Result:**

- Status Code: `200 OK`
- Content-Type: `text/plain`
- Body contains Prometheus-format metrics

---

## TC-015: CORS Headers Present

| Field | Value |
|-------|-------|
| **ID** | TC-015 |
| **Title** | CORS headers are returned on requests |
| **Priority** | Low |
| **Endpoint** | `OPTIONS /events` |

**Steps:**

1. Send:
   ```bash
   curl -X OPTIONS http://localhost:5000/events \
     -H "Origin: http://example.com" \
     -H "Access-Control-Request-Method: POST" -v
   ```

**Expected Result:**

- Response includes `Access-Control-Allow-Origin` header

---

## Test Execution Checklist

| ID | Title | Pass/Fail | Notes |
|----|-------|-----------|-------|
| TC-001 | Health check liveness | ☐ | |
| TC-002 | Readiness (service down) | ☐ | |
| TC-003 | Valid USER_REGISTERED | ☐ | |
| TC-004 | Valid PAYMENT_FAILED | ☐ | |
| TC-005 | Invalid event type | ☐ | |
| TC-006 | Missing email | ☐ | |
| TC-007 | Invalid email format | ☐ | |
| TC-008 | Missing userId | ☐ | |
| TC-009 | Future timestamp | ☐ | |
| TC-010 | Multiple validation errors | ☐ | |
| TC-011 | Get all events | ☐ | |
| TC-012 | Get event not found | ☐ | |
| TC-013 | Swagger UI accessible | ☐ | |
| TC-014 | Prometheus metrics | ☐ | |
| TC-015 | CORS headers | ☐ | |
