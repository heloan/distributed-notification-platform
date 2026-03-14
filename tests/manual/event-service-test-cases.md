# Event Service (Java) — Manual Test Cases

## Prerequisites
- Event Service running at `http://localhost:8080`
- PostgreSQL running with `events` table initialised

---

## TC-ES-001: Health Check
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `GET http://localhost:8080/actuator/health` | 200, `{"status":"UP"}` |

---

## TC-ES-002: Create Event
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST `/api/events` with `{"eventType":"USER_REGISTERED","userId":"u1","email":"u@e.com"}` | 201 Created, response has `id` |
| 2 | Verify `createdAt` timestamp in response | ISO-8601 format |

---

## TC-ES-003: Get All Events
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `GET /api/events` | 200, JSON array |
| 2 | Verify events are ordered by `createdAt` DESC | Newest first |

---

## TC-ES-004: Get Event by ID
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create an event, note the `id` | 201 |
| 2 | `GET /api/events/{id}` | 200, matching event |

---

## TC-ES-005: Get Non-existent Event
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `GET /api/events/00000000-0000-0000-0000-000000000000` | 404 Not Found |

---

## TC-ES-006: Kafka Event Publishing
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create an event via POST | 201 |
| 2 | Check Kafka topic `events` (use kafka-console-consumer) | Event message present |
| 3 | Verify message contains: `eventType`, `userId`, `email`, `id` | All fields present |

---

## TC-ES-007: Database Persistence
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create an event via POST | 201 |
| 2 | Query PostgreSQL: `SELECT * FROM events WHERE id = '{id}'` | Row exists |
| 3 | Verify all columns populated | `event_type`, `user_id`, `email`, `created_at` present |

---

## TC-ES-008: Validation — Invalid Payload
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST with unsupported `eventType` | 400 |
| 2 | POST with missing `userId` | 400 |
| 3 | POST with empty body | 400 |

---

## TC-ES-009: Prometheus Metrics
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | `GET http://localhost:8080/actuator/prometheus` | 200, Prometheus text format |
| 2 | Verify JVM and HTTP metrics present | Metrics visible |
