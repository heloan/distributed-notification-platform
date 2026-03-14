# 🏗️ Architecture

## High-Level Architecture

The Distributed Smart Notification Platform follows a **microservices architecture** with **event-driven communication**. Services are loosely coupled through a message broker (Apache Kafka) and each service has a single, well-defined responsibility.

```
                           ┌─────────────────────────┐
                           │        Clients           │
                           │   Web / Mobile / API     │
                           └────────────┬────────────┘
                                        │
                                   HTTP REST
                                        │
                           ┌────────────▼────────────┐
                           │      API Gateway         │
                           │      (.NET 8)            │
                           │                          │
                           │  • Request Validation    │
                           │  • Routing               │
                           │  • Authentication-Ready  │
                           └────────────┬────────────┘
                                        │
                           ┌────────────▼────────────┐
                           │     Event Service        │
                           │   (Java Spring Boot)     │
                           │                          │
                           │  • Receive Events        │
                           │  • Persist Events        │
                           │  • Publish Domain Events │
                           └────────────┬────────────┘
                                        │
                                  Publish Event
                                        │
                           ┌────────────▼────────────┐
                           │     Message Broker       │
                           │    Apache Kafka          │
                           │                          │
                           │  Event Streaming         │
                           │  Backbone                │
                           └────────────┬────────────┘
                                        │
                                  Consume Event
                                        │
                           ┌────────────▼────────────┐
                           │  Notification Service    │
                           │     (.NET Worker)        │
                           │                          │
                           │  • Event Consumer        │
                           │  • Rule Engine           │
                           │  • Notification Dispatch │
                           └────┬───────┬───────┬────┘
                                │       │       │
                           ┌────▼─┐ ┌───▼──┐ ┌──▼───┐
                           │Email │ │Slack │ │ SMS  │
                           └──────┘ └──────┘ └──────┘


              ─────────────────────────────────────────
                           DATA LAYER
              ─────────────────────────────────────────

                         ┌──────────────┐
                         │  PostgreSQL  │
                         │              │
                         │  Events      │
                         │  Notifications│
                         └──────────────┘


              ─────────────────────────────────────────
                        OBSERVABILITY STACK
              ─────────────────────────────────────────

                ┌────────────┐      ┌────────────┐
                │ Prometheus │─────▶│  Grafana   │
                │  Metrics   │      │ Dashboards │
                └────────────┘      └────────────┘
                      ▲
                 OpenTelemetry
                      │
          ┌───────────┴────────────┐
     Java Event Svc         .NET Notification Svc
```

---

## Architecture Principles

### 1. Microservices

Each service is **independently deployable**, has its own codebase, and focuses on a single business capability:

| Service | Technology | Responsibility |
|---------|-----------|---------------|
| API Gateway | .NET 8 | Entry point, validation, routing |
| Event Service | Java 21 / Spring Boot | Event ingestion and publishing |
| Notification Service | .NET 8 | Event processing and notification dispatch |

### 2. Event-Driven Architecture

Instead of synchronous service-to-service calls, services communicate through **events**:

```
Event Service  ──publish──▶  Kafka  ──consume──▶  Notification Service
```

**Benefits:**
- **Loose coupling** — Services don't know about each other
- **Scalability** — Consumers scale independently
- **Resilience** — Events are persisted in the broker
- **Asynchronous processing** — Non-blocking operations

### 3. Polyglot Microservices

Different services use the best technology for their needs:

| Service | Language | Rationale |
|---------|----------|-----------|
| Event Service | Java | Strong ecosystem for event processing |
| Notification Service | .NET | Excellent Worker Service model |
| API Gateway | .NET | Lightweight Minimal API |

### 4. Clean Architecture

Both the **Event Service** (Java) and the **Notification Service** (.NET) follow **Clean Architecture** with strict dependency rules:

```
┌─────────────────────────────────────────┐
│              Infrastructure             │
│  (Messaging, Email, Slack, Database)    │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │           Application            │  │
│  │   (Use Cases, Interfaces)        │  │
│  │                                   │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │          Domain            │  │  │
│  │  │  (Entities, Value Objects) │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘

Dependency Rule: Infrastructure → Application → Domain
```

### 5. SOLID Principles

- **S** — Single Responsibility: Each service and class has one job
- **O** — Open/Closed: Channel providers are extensible without modification
- **L** — Liskov Substitution: Providers are interchangeable via interfaces
- **I** — Interface Segregation: Lean, specific interfaces per concern
- **D** — Dependency Inversion: High-level modules depend on abstractions

---

## Communication Patterns

### Synchronous (HTTP REST)

```
Client ──HTTP──▶ API Gateway ──HTTP──▶ Event Service
```

Used for:
- Event submission
- Health checks
- Event querying

### Asynchronous (Kafka Events)

```
Event Service ──publish──▶ Kafka Topic ──consume──▶ Notification Service
```

Used for:
- Event-to-notification processing
- Decoupled service communication

---

## Deployment Architecture

All services run as **Docker containers** orchestrated by **Docker Compose**:

| Container | Port | Description |
|-----------|------|-------------|
| `api-gateway` | 5000 | .NET API Gateway |
| `event-service` | 8080 | Java Event Service |
| `notification-service` | — | .NET Worker (no HTTP port) |
| `postgres` | 5432 | PostgreSQL database |
| `kafka` | 9092 | Apache Kafka broker |
| `zookeeper` | 2181 | Kafka coordination |
| `prometheus` | 9090 | Metrics collection |
| `grafana` | 3000 | Dashboards |

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Monorepo | All services in one repo for easier navigation and portfolio display |
| Kafka over RabbitMQ | Better for event streaming and replay scenarios |
| .NET Worker Service | Ideal for long-running background consumers |
| Minimal API for Gateway | Lightweight, fast, modern .NET pattern |
| PostgreSQL | Reliable, JSONB support for event payloads |
| OpenTelemetry | Vendor-neutral observability standard |
| Jenkins CI/CD | Industry-standard pipeline with declarative syntax, quality gates, and staged deployments |
