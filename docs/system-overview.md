# 📘 System Overview

## Distributed Smart Notification Platform — Functional Specification

---

## 1. Project Objective

The Distributed Smart Notification Platform processes application events and **automatically triggers notifications** based on predefined rules. It demonstrates a production-style microservices system using polyglot technologies (.NET + Java).

---

## 2. Problem Statement

Modern applications generate a high volume of events:
- New user registered
- Payment approved or failed
- Order shipped
- System security alert

Each event may require notifying one or more users through different channels (email, Slack, SMS). Handling this in a monolithic application creates tight coupling and scaling limitations.

---

## 3. Solution

A **distributed, event-driven platform** where:

1. An **API Gateway** receives external requests
2. An **Event Service** ingests and persists events, then publishes them to a message broker
3. A **Notification Service** consumes events, evaluates business rules, and dispatches notifications through the appropriate channel

---

## 4. System Workflow

### 4.1 Happy Path — User Registration

```
1. Client sends POST /events with eventType: USER_REGISTERED
2. API Gateway validates and forwards the request
3. Event Service persists the event to PostgreSQL
4. Event Service publishes the event to Kafka topic "events"
5. Notification Service consumes the event
6. Rule Engine determines: USER_REGISTERED → Email
7. Email Provider sends a welcome email to the user
8. Notification record is persisted with status SENT
```

### 4.2 Event Types and Rules

| Event Type | Channel | Notification |
|-----------|---------|-------------|
| `USER_REGISTERED` | Email | Welcome email |
| `PAYMENT_FAILED` | Slack | Payment failure alert |
| `ORDER_SHIPPED` | SMS | Shipment tracking notification |
| `SECURITY_ALERT` | Email + Slack | Security breach notification |

---

## 5. Service Specifications

### 5.1 API Gateway (.NET 8)

**Purpose:** Single entry point for all external requests.

**Endpoints:**

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/events` | Submit a new event |
| `GET` | `/events` | List events (proxied) |
| `GET` | `/health` | Gateway health status |

**Responsibilities:**
- Request validation
- Request routing to downstream services
- Authentication-ready structure
- Structured logging

### 5.2 Event Service (Java 21 / Spring Boot)

**Purpose:** Event ingestion, persistence, and publishing.

**Endpoints:**

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/events` | Create and publish an event |
| `GET` | `/events` | Retrieve all events |
| `GET` | `/events/{id}` | Retrieve a single event |

**Event Payload:**

```json
{
  "eventType": "USER_REGISTERED",
  "userId": "123",
  "email": "user@email.com",
  "timestamp": "2026-03-14T10:15:00"
}
```

**Responsibilities:**
- Validate event data
- Persist event to PostgreSQL
- Publish event to Kafka topic `events`

**Architecture:** Clean Architecture with Hexagonal Ports & Adapters.

```
Domain
  ├── Entity (Event — rich domain model)
  ├── Enums (EventType)
  ├── Events (EventCreatedDomainEvent)
  ├── Exceptions (EventNotFoundException, EventValidationException)
  └── Repository port (EventRepository)

Application
  ├── DTOs (CreateEventRequest, EventResponse, ErrorResponse)
  ├── Mapper (EventMapper)
  ├── Output Port (EventPublisher)
  └── Use Cases (CreateEvent, GetEventById, GetAllEvents)

Infrastructure
  ├── Persistence (EventJpaEntity, JpaEventRepositoryAdapter)
  ├── Messaging (KafkaEventPublisher)
  └── Config (Jackson, Kafka, OpenAPI)

API
  ├── Controller (EventController)
  └── Exception (GlobalExceptionHandler)
```

### 5.3 Notification Service (.NET 8 Worker)

**Purpose:** Event consumption, rule evaluation, and notification dispatch.

**Architecture:** Clean Architecture with DDD-style layers.

```
Domain
  ├── Entities (Notification, NotificationRule)
  ├── ValueObjects (Channel, NotificationStatus)
  └── Enums (EventType, ChannelType)

Application
  ├── UseCases (ProcessEventUseCase)
  └── Interfaces (INotificationSender, IRuleEngine)

Infrastructure
  ├── Messaging (KafkaConsumer)
  ├── Providers (EmailProvider, SlackProvider, SmsProvider)
  └── Persistence (NotificationRepository)
```

**Processing Flow:**

```
Event Consumed from Kafka
        ↓
ProcessEventUseCase
        ↓
NotificationRuleEngine.Evaluate(event)
        ↓
Channel determined (Email / Slack / SMS)
        ↓
INotificationSender.Send(notification)
        ↓
Notification persisted with status
```

---

## 6. Non-Functional Requirements

| Requirement | Implementation |
|-------------|---------------|
| **Scalability** | Services scale independently via containers |
| **Observability** | OpenTelemetry + Prometheus + Grafana |
| **Reliability** | Kafka ensures event durability |
| **Maintainability** | Clean Architecture, SOLID, clear boundaries |
| **Portability** | Docker Compose for local development |
| **Extensibility** | New channels added by implementing an interface |

---

## 7. Key Metrics

| Metric | Description |
|--------|-------------|
| `events_processed_total` | Total number of events ingested |
| `notifications_sent_total` | Total notifications dispatched |
| `notification_processing_time` | Time from event received to notification sent |
| `notification_failures_total` | Failed notification attempts |

---

## 8. Testing Strategy

### 8.1 Testing Layers

| Layer | Tool | Language | Scope |
|-------|------|----------|-------|
| **Unit Tests** | xUnit / JUnit | C# / Java | Domain rules, validators, services |
| **Integration Tests** | pytest + requests | Python | Live API endpoints, service communication |
| **API Automation** | Robot Framework | Robot / Python | End-to-end API automation with keywords |
| **Browser Tests** | Selenium (SeleniumLibrary) | Python / Robot | Swagger UI validation, page rendering |
| **Manual Tests** | Documented procedures | Markdown | Step-by-step test cases with checklist |

### 8.2 Test Tools & Frameworks

| Tool | Purpose |
|------|---------|
| **xUnit + Moq + FluentAssertions** | .NET unit & integration tests |
| **JUnit + Mockito** | Java unit & integration tests |
| **pytest + requests** | Python HTTP integration tests |
| **Robot Framework + RequestsLibrary** | Keyword-driven API test automation |
| **Selenium + SeleniumLibrary** | Browser-based UI testing |
| **pytest-html** | HTML test reports for pytest |
| **Robot Framework reports** | Built-in HTML/XML reporting |

### 8.3 Example Test Scenarios

**Validation Tests:**
- Invalid event type → `400 Bad Request` with error details
- Empty email → `400 Bad Request`
- Future timestamp → `400 Bad Request`
- Multiple invalid fields → all errors returned at once

**Forwarding Tests:**
- `USER_REGISTERED` event → `202 Accepted` (Event Service up)
- Valid event → `502 Bad Gateway` (Event Service down)
- Non-existent event ID → `404 Not Found`

**Observability Tests:**
- `/metrics` → Prometheus-format metrics returned
- `/swagger/v1/swagger.json` → valid OpenAPI spec
- Swagger UI → page renders with all endpoints visible

**End-to-End Tests:**
- `USER_REGISTERED` event → email notification created and sent
- `PAYMENT_FAILED` event → Slack alert dispatched
- Unknown event type → notification skipped, logged
- Duplicate event → idempotent processing

### 8.4 Test Structure

```
tests/                              # Root-level black-box tests
├── integration/                    # pytest + requests
│   ├── gateway/                    # API Gateway tests
│   ├── event-service/              # Event Service tests
│   ├── notification-service/       # Notification Service tests
│   └── end-to-end/                # Full pipeline tests
├── robot/                          # Robot Framework
│   ├── gateway/
│   ├── event-service/
│   └── end-to-end/
├── selenium/                       # Browser tests
├── manual/                         # Manual test cases
├── scripts/                        # Runner scripts
└── docker-compose.test.yml         # Test infrastructure

services/*/tests/                   # Unit tests (inside each service)
```

> **Architecture decision:** Unit tests live inside each service because they are coupled to implementation. Black-box tests (integration, robot, selenium, manual) live at the root `/tests/` directory because they test services through HTTP, independent of implementation language.

### 8.5 Running Tests

```bash
# All black-box tests
./tests/scripts/run-all-tests.sh

# Individual suites
./tests/scripts/run-integration-tests.sh          # Python pytest
./tests/scripts/run-integration-tests.sh -m gateway  # Gateway only
./tests/scripts/run-robot-tests.sh                # Robot Framework
./tests/scripts/run-selenium-tests.sh             # Selenium browser
./tests/scripts/run-e2e-tests.sh                  # End-to-end only

# Unit tests (per service)
cd services/api-gateway-dotnet && dotnet test
```

---

## 9. CI/CD Pipeline

The project uses a **Jenkins Declarative Pipeline** (`Jenkinsfile`) with the following stages:

| # | Stage | Execution | Purpose |
|---|-------|-----------|---------|
| 1 | Checkout | Sequential | Clone repo, print environment info |
| 2 | Static Analysis | Parallel | `dotnet format`, Checkstyle, `flake8` |
| 3 | Build | Parallel | Compile .NET + Java services |
| 4 | Unit Tests | Parallel | xUnit + JUnit per service |
| 5 | Docker Build | Sequential | Multi-stage image builds |
| 6 | Integration Tests | Parallel | pytest + Robot Framework against Compose infra |
| 7 | Quality Gate | Sequential | Enforce zero failures, pass-rate thresholds |
| 8 | Docker Push | Sequential | Push to registry (protected branches only) |
| 9 | Deploy — Staging | Sequential | Auto-deploy on `develop`/`release/*` |
| 10 | Deploy — Production | Sequential | Manual approval on `main` |

### Branching Strategy (GitFlow)

| Branch | Build | Tests | Push | Deploy |
|--------|-------|-------|------|--------|
| `feat/*` | ✅ | ✅ | ❌ | ❌ |
| `develop` | ✅ | ✅ | ✅ | Staging |
| `release/*` | ✅ | ✅ | ✅ | Staging |
| `main` | ✅ | ✅ | ✅ | Production |

> 📖 Full documentation: [docs/ci-cd-pipeline.md](ci-cd-pipeline.md)

---

## 10. Security Considerations (Future)

| Concern | Approach |
|---------|----------|
| Authentication | JWT token validation at API Gateway |
| Authorization | Role-based access control |
| Secrets | Environment variables / secret manager |
| Input validation | DTO validation at every service boundary |

---

## 11. Glossary

| Term | Definition |
|------|-----------|
| **Event** | A record of something that happened in the system |
| **Notification** | A message sent to a user through a channel |
| **Channel** | The delivery method: Email, Slack, or SMS |
| **Rule** | A mapping from event type to notification channel |
| **Broker** | The message streaming platform (Apache Kafka) |
| **Consumer** | A service that reads events from the broker |
| **Producer** | A service that publishes events to the broker |
