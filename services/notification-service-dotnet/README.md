# Notification Service (.NET 8 Worker)

> **Event-driven notification processing service** — consumes events from Kafka, evaluates notification rules, and dispatches through Email, Slack, and SMS channels.

| Aspect | Detail |
|--------|--------|
| **Language / Runtime** | C# / .NET 8 |
| **Host** | Worker Service + Minimal Kestrel (health endpoints) |
| **Architecture** | Clean Architecture + SOLID + DDD-style layers |
| **Persistence** | PostgreSQL 16 via EF Core 8 |
| **Messaging** | Apache Kafka (Confluent .NET client) |
| **Observability** | Serilog structured logging, health checks |
| **Build** | `dotnet` CLI / MSBuild |
| **Container** | Multi-stage Docker (SDK build → ASP.NET runtime) |

---

## Endpoints

| Method | Path | Description | Status |
|--------|------|-------------|--------|
| `GET` | `/health` | Health check | `200` |
| `GET` | `/metrics` | Basic metrics | `200` |

> This is a **Worker Service** — it has no REST API for business operations. It consumes events from Kafka and processes them asynchronously.

---

## Notification Rules

| Event Type | Channel(s) | Message |
|-----------|------------|---------|
| `USER_REGISTERED` | 📧 Email | Welcome to the platform! |
| `PAYMENT_FAILED` | 💬 Slack | Payment processing failed alert |
| `ORDER_SHIPPED` | 📱 SMS | Shipment tracking notification |
| `SECURITY_ALERT` | 📧 Email + 💬 Slack | Security breach notification |

---

## Clean Architecture

```
┌──────────────────────────────────────────────────┐
│              Worker Layer (Host)                  │
│     (Program.cs, EventConsumerWorker)             │
├──────────────────────────────────────────────────┤
│             Infrastructure Layer                  │
│  (Kafka Consumer, EF Core, Providers, Rules)      │
├──────────────────────────────────────────────────┤
│              Application Layer                    │
│   (Use Cases, DTOs, Interfaces / Ports)           │
├──────────────────────────────────────────────────┤
│               Domain Layer                        │
│  (Entities, Enums, Exceptions, Repository Ports)  │
└──────────────────────────────────────────────────┘
```

> **Dependency Rule** — outer layers depend on inner layers, never the reverse. Domain has zero external dependencies.

### SOLID Principles Applied

| Principle | Application |
|-----------|-------------|
| **S** – Single Responsibility | Each use case handles one workflow; providers handle one channel |
| **O** – Open/Closed | New channels added by implementing `INotificationSender` — no existing code changes |
| **L** – Liskov Substitution | All providers (`Email`, `Slack`, `SMS`) are interchangeable via `INotificationSender` |
| **I** – Interface Segregation | Separate `IRuleEngine`, `INotificationSender`, `IEventConsumer`, `INotificationRepository` |
| **D** – Dependency Inversion | Domain defines ports; infrastructure provides adapters (EF Core, Kafka, providers) |

---

## Processing Flow

```
Kafka Topic "events"
        │
        ▼
EventConsumerWorker (BackgroundService)
        │
        ▼
ProcessEventUseCase.ExecuteAsync(EventMessage)
        │
        ├── 1. Parse EventType enum
        ├── 2. IRuleEngine.Evaluate(eventType) → NotificationRule
        ├── 3. Notification.Create(...) → PENDING
        ├── 4. INotificationSender.SendAsync(notification)
        └── 5. Update status → SENT / FAILED
```

---

## Project Structure

```
src/
├── NotificationService.Domain/
│   ├── Entities/
│   │   ├── Notification.cs              # Aggregate root with state transitions
│   │   └── NotificationRule.cs          # Rule mapping event → channels + template
│   ├── Enums/
│   │   ├── EventType.cs
│   │   ├── ChannelType.cs
│   │   └── NotificationStatus.cs
│   ├── Exceptions/
│   │   ├── NotificationDomainException.cs
│   │   └── RuleNotFoundException.cs
│   └── Repositories/
│       └── INotificationRepository.cs   # Persistence port
│
├── NotificationService.Application/
│   ├── DTOs/
│   │   ├── EventMessage.cs              # Kafka event payload
│   │   └── NotificationResponse.cs      # Outbound DTO
│   ├── Interfaces/
│   │   ├── IRuleEngine.cs               # Rule evaluation port
│   │   ├── INotificationSender.cs       # Channel dispatch port
│   │   └── IEventConsumer.cs            # Messaging port
│   ├── Mappers/
│   │   └── NotificationMapper.cs
│   └── UseCases/
│       ├── ProcessEventUseCase.cs       # Core business workflow
│       └── GetNotificationsUseCase.cs   # Read notifications
│
├── NotificationService.Infrastructure/
│   ├── Persistence/
│   │   ├── NotificationDbContext.cs     # EF Core DbContext
│   │   └── EfNotificationRepository.cs # Repository adapter
│   ├── Messaging/
│   │   └── KafkaEventConsumer.cs        # Kafka consumer adapter
│   ├── RuleEngine/
│   │   └── InMemoryRuleEngine.cs        # Business rules
│   ├── Providers/
│   │   ├── EmailProvider.cs
│   │   ├── SlackProvider.cs
│   │   └── SmsProvider.cs
│   ├── Configuration/
│   │   ├── KafkaSettings.cs
│   │   └── DatabaseSettings.cs
│   └── DependencyInjection.cs           # DI wiring
│
└── NotificationService.Worker/
    ├── Program.cs                       # Host entry point
    ├── EventConsumerWorker.cs           # Background service
    ├── appsettings.json
    └── appsettings.Development.json

tests/
└── NotificationService.Tests/
    ├── Domain/Entities/
    │   ├── NotificationTest.cs          # 8 tests
    │   └── NotificationRuleTest.cs      # 5 tests
    ├── Application/
    │   ├── UseCases/
    │   │   ├── ProcessEventUseCaseTest.cs   # 6 tests
    │   │   └── GetNotificationsUseCaseTest.cs # 3 tests
    │   └── Mappers/
    │       └── NotificationMapperTest.cs    # 2 tests
    └── Infrastructure/RuleEngine/
        └── InMemoryRuleEngineTest.cs        # 4 tests
```

---

## Running Locally

```bash
# Prerequisites: .NET 8 SDK, Docker (for Postgres + Kafka)
cd services/notification-service-dotnet

# Start dependencies
docker compose -f ../../infrastructure/docker-compose.yml up -d postgres kafka zookeeper

# Run the service
dotnet run --project src/NotificationService.Worker

# Run tests
dotnet test

# Build Docker image
docker build -t dsnp/notification-service:latest .
```

---

## Testing Strategy

| Layer | Tool | Scope | Tests |
|-------|------|-------|-------|
| Domain | xUnit + FluentAssertions | Entity invariants, state transitions, rules | 13 |
| Application | xUnit + Moq + FluentAssertions | Use case orchestration, mapper | 11 |
| Infrastructure | xUnit + FluentAssertions | Rule engine evaluation | 4 |
| **Total** | | | **28** |

---

## Kafka Event Schema

Consumed from topic **`events`**:

```json
{
  "eventId": "550e8400-e29b-41d4-a716-446655440000",
  "eventType": "USER_REGISTERED",
  "userId": "usr-42",
  "email": "user@example.com",
  "occurredAt": "2024-01-15T10:30:00Z"
}
```

---

## Status

✅ **Implemented** — domain model, use cases, Kafka consumer, channel providers, rule engine, EF Core persistence, unit tests, Docker packaging.
