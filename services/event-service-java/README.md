# Event Service (Java 21 / Spring Boot)

> **Domain-event ingestion micro-service** — validates, persists, and publishes platform events to Apache Kafka.

| Aspect | Detail |
|--------|--------|
| **Language / Runtime** | Java 21 (Eclipse Temurin) |
| **Framework** | Spring Boot 3.2.3 |
| **Architecture** | Clean Architecture + Hexagonal Ports & Adapters |
| **Persistence** | PostgreSQL 16 via Spring Data JPA |
| **Messaging** | Apache Kafka (Spring Kafka) |
| **Docs** | SpringDoc OpenAPI 3 (Swagger UI) |
| **Observability** | Micrometer → Prometheus, Structured JSON logging |
| **Build** | Maven 3.9.6 (wrapper included) |
| **Container** | Multi-stage Docker (Temurin 21 JDK build → JRE runtime) |

---

## Endpoints

| Method | Path | Description | Status |
|--------|------|-------------|--------|
| `POST` | `/api/events` | Create a new event | `201` |
| `GET` | `/api/events/{id}` | Get event by ID | `200` / `404` |
| `GET` | `/api/events` | List all events | `200` |
| `GET` | `/actuator/health` | Health check | `200` |
| `GET` | `/actuator/prometheus` | Prometheus metrics | `200` |
| `GET` | `/swagger-ui.html` | Interactive API docs | `200` |

### Create Event Payload

```json
{
  "eventType": "USER_REGISTERED",
  "userId": "usr-42",
  "email": "jane@example.com",
  "timestamp": "2024-01-15T10:30:00Z"   // optional
}
```

### Supported Event Types

| Type | Description |
|------|-------------|
| `USER_REGISTERED` | New user sign-up |
| `PAYMENT_FAILED` | Payment processing failure |
| `ORDER_SHIPPED` | Order dispatched |
| `SECURITY_ALERT` | Suspicious activity detected |

---

## Clean Architecture

```
┌──────────────────────────────────────────────────┐
│                   API Layer                       │
│         (Controllers, Exception Handlers)         │
├──────────────────────────────────────────────────┤
│               Application Layer                   │
│   (Use Cases, DTOs, Mappers, Output Ports)        │
├──────────────────────────────────────────────────┤
│                 Domain Layer                       │
│  (Entities, Value Objects, Repository Ports)       │
├──────────────────────────────────────────────────┤
│             Infrastructure Layer                   │
│  (JPA Adapters, Kafka Publisher, Spring Config)    │
└──────────────────────────────────────────────────┘
```

> **Dependency Rule** — outer layers depend on inner layers, never the reverse.

### SOLID Principles Applied

| Principle | Application |
|-----------|-------------|
| **S** – Single Responsibility | Each use case handles one operation; controller only delegates |
| **O** – Open/Closed | New event types added to enum without touching use cases |
| **L** – Liskov Substitution | `EventRepository` port is swappable (JPA, in-memory, mock) |
| **I** – Interface Segregation | Separate `EventRepository` (domain) and `EventPublisher` (application) ports |
| **D** – Dependency Inversion | Domain defines ports; infrastructure provides adapters |

---

## Project Structure

```
src/
├── main/
│   ├── java/com/dsnp/eventservice/
│   │   ├── EventServiceApplication.java
│   │   ├── domain/
│   │   │   ├── entity/Event.java
│   │   │   ├── enums/EventType.java
│   │   │   ├── event/EventCreatedDomainEvent.java
│   │   │   ├── exception/
│   │   │   │   ├── EventNotFoundException.java
│   │   │   │   └── EventValidationException.java
│   │   │   └── repository/EventRepository.java
│   │   ├── application/
│   │   │   ├── dto/
│   │   │   │   ├── CreateEventRequest.java
│   │   │   │   ├── EventResponse.java
│   │   │   │   └── ErrorResponse.java
│   │   │   ├── mapper/EventMapper.java
│   │   │   ├── port/EventPublisher.java
│   │   │   └── usecase/
│   │   │       ├── CreateEventUseCase.java
│   │   │       ├── GetEventByIdUseCase.java
│   │   │       └── GetAllEventsUseCase.java
│   │   ├── infrastructure/
│   │   │   ├── persistence/
│   │   │   │   ├── EventJpaEntity.java
│   │   │   │   ├── SpringDataEventRepository.java
│   │   │   │   └── JpaEventRepositoryAdapter.java
│   │   │   ├── messaging/KafkaEventPublisher.java
│   │   │   └── config/
│   │   │       ├── JacksonConfig.java
│   │   │       ├── KafkaTopicConfig.java
│   │   │       └── OpenApiConfig.java
│   │   └── api/
│   │       ├── controller/EventController.java
│   │       └── exception/GlobalExceptionHandler.java
│   └── resources/
│       ├── application.yml
│       └── application-dev.yml
└── test/
    ├── java/com/dsnp/eventservice/
    │   ├── domain/
    │   │   ├── entity/EventTest.java
    │   │   └── enums/EventTypeTest.java
    │   ├── application/
    │   │   ├── usecase/CreateEventUseCaseTest.java
    │   │   ├── usecase/GetEventByIdUseCaseTest.java
    │   │   ├── usecase/GetAllEventsUseCaseTest.java
    │   │   └── mapper/EventMapperTest.java
    │   └── api/controller/EventControllerTest.java
    └── resources/
        └── application-test.yml
```

---

## Running Locally

```bash
# Prerequisites: Java 21, Docker (for Postgres + Kafka)
cd services/event-service-java

# Start dependencies
docker compose -f ../../infrastructure/docker-compose.yml up -d postgres kafka zookeeper

# Run with dev profile
./mvnw spring-boot:run -Dspring-boot.run.profiles=dev

# Run tests
./mvnw test

# Build Docker image
docker build -t dsnp/event-service:latest .
```

---

## Testing Strategy

| Layer | Tool | Scope |
|-------|------|-------|
| Domain | JUnit 5 + AssertJ | Entity invariants, enum parsing |
| Application | JUnit 5 + Mockito | Use case orchestration, mapper |
| API | MockMvc + @WebMvcTest | HTTP contract, validation, error responses |
| Integration | Testcontainers (planned) | Full stack with real Postgres + Kafka |

---

## Kafka Event Schema

Published to topic **`events`** on every successful event creation:

```json
{
  "eventId": "550e8400-e29b-41d4-a716-446655440000",
  "eventType": "USER_REGISTERED",
  "userId": "usr-42",
  "occurredAt": "2024-01-15T10:30:00Z"
}
```

---

## Status

✅ **Implemented** — domain model, use cases, REST API, Kafka publishing, unit tests, Docker packaging.
