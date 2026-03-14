# End-to-End — Manual Test Cases

## Prerequisites
- **All** services running: Gateway, Event Service, Notification Service
- Infrastructure up: PostgreSQL, Kafka, Zookeeper, Prometheus, Grafana
- Start everything: `docker compose up -d` from `infrastructure/`

---

## TC-E2E-001: Full Event Pipeline
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST `/api/events` to Gateway with `USER_REGISTERED` | 2xx success |
| 2 | `GET /api/events/{id}` on Event Service | 200, event exists |
| 3 | Wait 5 seconds for Kafka consumer | — |
| 4 | `GET /api/notifications` on Notification Service | Notification created for the event |

---

## TC-E2E-002: Multi-Channel Notification Routing
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST `USER_REGISTERED` event | Email notification dispatched |
| 2 | POST `PAYMENT_FAILED` event | SMS notification dispatched |
| 3 | POST `ORDER_SHIPPED` event | Push notification dispatched |
| 4 | POST `SECURITY_ALERT` event | Push + Email notification dispatched |

---

## TC-E2E-003: High Volume — Burst Test
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Send 50 events rapidly via Gateway | All return 2xx |
| 2 | Wait 30 seconds | — |
| 3 | Check Event Service count | 50 events persisted |
| 4 | Check Notification Service count | ≥ 50 notifications created |

---

## TC-E2E-004: Service Failure & Recovery
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Stop Event Service container | Container stopped |
| 2 | POST event to Gateway | 502/503 — circuit breaker |
| 3 | Restart Event Service | Container running |
| 4 | Wait 30s, POST event to Gateway | 2xx success — circuit recovered |

---

## TC-E2E-005: Kafka Failure & Recovery
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Stop Kafka container | Container stopped |
| 2 | POST event to Gateway | Event Service may accept but not publish to Kafka |
| 3 | Restart Kafka container | Container running |
| 4 | Verify pending events are eventually consumed | Notification Service catches up |

---

## TC-E2E-006: Observability Verification
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Send 10 events through Gateway | All succeed |
| 2 | Open Grafana dashboard at `http://localhost:3000` | Dashboard loads |
| 3 | Verify request count panel shows increase | Counter incremented |
| 4 | Verify response time panel shows data | Histogram populated |
| 5 | Check Prometheus at `http://localhost:9090/targets` | All targets UP |

---

## TC-E2E-007: Data Consistency
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | POST unique event with known `userId` | 2xx |
| 2 | Query Event Service DB: `SELECT * FROM events WHERE user_id = '...'` | Row exists |
| 3 | Query Notification DB: `SELECT * FROM notifications WHERE user_id = '...'` | Row exists |
| 4 | Verify `event_id` foreign key matches | Consistent across services |
