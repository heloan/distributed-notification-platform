<h1 align="center">Distributed Smart Notification Platform</h1>

<p align="center">
  A polyglot microservices system that processes application events and automatically sends notifications through multiple channels.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Java-21-ED8B00?logo=openjdk&logoColor=white" alt="Java 21" />
  <img src="https://img.shields.io/badge/Spring%20Boot-3.x-6DB33F?logo=springboot&logoColor=white" alt="Spring Boot" />
  <img src="https://img.shields.io/badge/Kafka-Streaming-231F20?logo=apachekafka&logoColor=white" alt="Kafka" />
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white" alt="Docker" />
  <img src="https://img.shields.io/badge/Jenkins-CI%2FCD-D24939?logo=jenkins&logoColor=white" alt="Jenkins" />
  <img src="https://img.shields.io/badge/License-MIT-green.svg" alt="MIT License" />
</p>

---

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Services](#services)
- [Tech Stack](#tech-stack)
- [Repository Structure](#repository-structure)
- [CI/CD Pipeline](#cicd-pipeline)
- [Getting Started](#getting-started)
- [API Reference](#api-reference)
- [Observability](#observability)
- [Documentation](#documentation)
- [Engineering Principles](#engineering-principles)
- [Future Improvements](#future-improvements)
- [License](#license)

---

## Overview

Companies generate many application events — user registrations, payment approvals, order shipments, security alerts. This platform receives those events and **automatically decides how and where to notify users**.

**Example workflow:**

```
User registers → Event created → Published to Kafka → Notification service processes rules → Email / Slack / SMS sent
```

The project demonstrates **senior-level engineering** practices including microservices architecture, event-driven communication, cross-platform development (.NET + Java), Clean Architecture, SOLID principles, and production-grade observability.

---

## Architecture

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
                           │  Validation · Routing    │
                           └────────────┬────────────┘
                                        │
                           ┌────────────▼────────────┐
                           │     Event Service        │
                           │   (Java Spring Boot)     │
                           │  Receive · Persist ·     │
                           │  Publish Events          │
                           └────────────┬────────────┘
                                        │
                                  Publish Event
                                        │
                           ┌────────────▼────────────┐
                           │     Message Broker       │
                           │    Apache Kafka          │
                           │  Event Streaming         │
                           └────────────┬────────────┘
                                        │
                                  Consume Event
                                        │
                           ┌────────────▼────────────┐
                           │  Notification Service    │
                           │     (.NET Worker)        │
                           │  Rules · Dispatch        │
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
                         │  Events ·    │
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

> 📖 See [docs/architecture.md](docs/architecture.md) for the full architecture breakdown.

---

## Services

| Service | Technology | Responsibility | Status |
|---------|-----------|---------------|--------|
| **API Gateway** | .NET 8 Minimal API | Entry point · Request validation · Routing | ✅ Implemented |
| **Event Service** | Java 21 · Spring Boot | Receive events · Persist · Publish to Kafka | ✅ Implemented |
| **Notification Service** | .NET 8 Worker Service | Consume events · Apply rules · Send notifications | ✅ Implemented |

### Event Types

| Event | Notification Channel |
|-------|---------------------|
| `USER_REGISTERED` | Welcome email |
| `PAYMENT_FAILED` | Slack alert |
| `ORDER_SHIPPED` | SMS notification |
| `SECURITY_ALERT` | Email + Slack |

---

## Tech Stack

| Category | Technologies |
|----------|-------------|
| **Backend** | .NET 8 · Java 21 · Spring Boot 3.x |
| **Messaging** | Apache Kafka |
| **Database** | PostgreSQL |
| **Infrastructure** | Docker · Docker Compose |
| **Observability** | OpenTelemetry · Prometheus · Grafana |
| **CI/CD** | Jenkins · Declarative Pipeline · Docker |
| **Architecture** | Clean Architecture · SOLID · DDD |

---

## Repository Structure

```
distributed-notification-platform/
│
├── services/
│   ├── api-gateway-dotnet/          # .NET 8 Minimal API Gateway
│   │   ├── src/                     # Clean Architecture layers
│   │   ├── tests/                   # Unit tests (xUnit)
│   │   └── Dockerfile
│   │
│   ├── event-service-java/          # Java 21 Spring Boot Event Service ✅
│   │   ├── src/main/java/.../       # Clean Architecture (domain → app → infra → api)
│   │   ├── src/test/java/.../       # Unit tests (JUnit 5 + Mockito)
│   │   └── Dockerfile
│   │
│   └── notification-service-dotnet/ # .NET 8 Worker Notification Service ✅
│       ├── src/                     # Clean Architecture (Domain → App → Infra → Worker)
│       ├── tests/                   # Unit tests (xUnit + Moq + FluentAssertions)
│       └── Dockerfile
│
├── tests/                           # ← Black-box tests (service-agnostic)
│   ├── integration/                 # Python + pytest HTTP tests
│   │   ├── gateway/                 # API Gateway integration tests
│   │   ├── event-service/           # Event Service integration tests
│   │   ├── notification-service/    # Notification Service integration tests
│   │   └── end-to-end/             # Full pipeline E2E tests
│   ├── robot/                       # Robot Framework API automation
│   │   ├── gateway/
│   │   ├── event-service/
│   │   └── end-to-end/
│   ├── selenium/                    # Browser UI tests (Swagger, Grafana)
│   ├── manual/                      # Manual test case documentation
│   ├── scripts/                     # Test runner scripts
│   └── docker-compose.test.yml      # Isolated test infrastructure
│
├── infrastructure/
│   ├── docker-compose.yml           # Full local environment
│   ├── prometheus/                  # Prometheus configuration
│   └── grafana/                     # Grafana dashboards & provisioning
│
├── docs/
│   ├── architecture.md              # Architecture documentation
│   ├── system-overview.md           # Functional specification
│   ├── database-design.md           # Database schema & ER diagram
│   └── uml-diagrams.md             # UML component & sequence diagrams
│
├── scripts/
│   ├── start.sh                     # Start all services
│   ├── stop.sh                      # Stop all services
│   └── build.sh                     # Build all services
│
├── Jenkinsfile                      # CI/CD pipeline definition
├── jenkins/                         # Jenkins agent, helpers, quality gates
│
├── .editorconfig                    # Cross-platform code style
├── .gitignore                       # .NET + Java + Infra ignores
├── LICENSE                          # MIT License
└── README.md                        # This file
```

---

## CI/CD Pipeline

The project uses a **Jenkins Declarative Pipeline** with 10 stages:

```
Checkout → Static Analysis → Build → Unit Tests → Docker Build → Integration Tests → Quality Gate → Docker Push → Staging → Production
```

| Stage | What it does |
|-------|--------------|
| **Static Analysis** | `dotnet format`, Checkstyle, `flake8` — parallel across languages |
| **Build** | Compile .NET + Java services in parallel |
| **Unit Tests** | xUnit + JUnit — parallel per service, results published to Jenkins |
| **Docker Build** | Multi-stage images for all implemented services |
| **Integration Tests** | Spin up infra via Compose → run pytest + Robot Framework in parallel → teardown |
| **Quality Gate** | Zero unit test failures, ≥ 95% integration pass rate |
| **Docker Push** | Push to registry on `main`, `develop`, `release/*` branches |
| **Deploy** | Staging (automatic) · Production (manual approval) |

> 📖 See [docs/ci-cd-pipeline.md](docs/ci-cd-pipeline.md) for the full pipeline documentation.

---

## Getting Started

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/install/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) *(for local development)*
- [Java 21](https://adoptium.net/) *(for local development)*

### Run the Full Platform

```bash
# Clone the repository
git clone https://github.com/heloan-marinho/distributed-notification-platform.git
cd distributed-notification-platform

# Start all services
docker compose -f infrastructure/docker-compose.yml up -d

# Or use the convenience script
./scripts/start.sh
```

### Send a Test Event

```bash
curl -X POST http://localhost:5000/events \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "USER_REGISTERED",
    "userId": "123",
    "email": "user@email.com",
    "timestamp": "2026-03-14T10:15:00"
  }'
```

### Stop Services

```bash
docker-compose -f infrastructure/docker-compose.yml down

# Or use the convenience script
./scripts/stop.sh
```

---

## API Reference

### API Gateway

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/events` | Submit a new event |
| `GET` | `/events` | List processed events |
| `GET` | `/health` | Health check |

### Event Payload

```json
{
  "eventType": "USER_REGISTERED",
  "userId": "123",
  "email": "user@email.com",
  "timestamp": "2026-03-14T10:15:00"
}
```

---

## Observability

| Tool | Purpose | URL |
|------|---------|-----|
| **Prometheus** | Metrics collection | `http://localhost:9090` |
| **Grafana** | Dashboards & visualization | `http://localhost:3000` |

### Key Metrics

- `events_processed_total` — Total events ingested
- `notifications_sent_total` — Total notifications dispatched
- `notification_processing_time` — End-to-end processing latency

---

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture](docs/architecture.md) | System architecture & design decisions |
| [System Overview](docs/system-overview.md) | Functional specification & workflows |
| [Database Design](docs/database-design.md) | Schema, ER diagrams, table definitions |
| [UML Diagrams](docs/uml-diagrams.md) | Component, sequence & class diagrams |
| [Test Suite](tests/README.md) | Testing strategy, tools & runner scripts |
| [CI/CD Pipeline](docs/ci-cd-pipeline.md) | Jenkins pipeline stages, quality gates & setup |

---

## Engineering Principles

This project demonstrates:

- **Microservices Architecture** — Independent, deployable services with clear boundaries
- **Event-Driven Communication** — Asynchronous messaging via Apache Kafka
- **Polyglot Backend** — Java (Spring Boot) + .NET cross-platform development
- **Clean Architecture** — Domain → Application → Infrastructure dependency rule
- **SOLID Principles** — Interface-driven design, single responsibility, dependency inversion
- **Containerization** — Docker Compose for reproducible local environments
- **CI/CD Pipeline** — Jenkins declarative pipeline with quality gates, parallel stages, and staged deployments
- **Observability** — OpenTelemetry + Prometheus + Grafana monitoring stack

---

## Future Improvements

- 🔐 Authentication & authorization (JWT)
- 🔄 Retry mechanism with exponential backoff
- 📬 Dead-letter queue for failed notifications
- 📝 Notification templates engine
- 🖥️ Web UI dashboard
- ☸️ Kubernetes deployment manifests
- 📊 Distributed tracing with Jaeger
- 📈 SonarQube integration for code quality
- 🔒 Trivy container image scanning in CI

---

## License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  Built with ❤️ by <a href="https://github.com/heloan-marinho">Heloan Marinho</a>
</p>
