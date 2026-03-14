# Notification Service (.NET 8 Worker)

> Event-driven notification processing service for the Distributed Smart Notification Platform.

## Responsibilities

- Consume events from Apache Kafka
- Evaluate notification rules (Rule Engine)
- Dispatch notifications through appropriate channels
- Persist notification history

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| .NET 8 | Runtime |
| Worker Service | Background processing |
| Clean Architecture | Architectural pattern |
| DDD | Domain modeling |
| Apache Kafka | Event consumption |
| PostgreSQL | Notification storage |
| OpenTelemetry | Observability |

## Notification Rules

| Event Type | Channel | Notification |
|-----------|---------|-------------|
| `USER_REGISTERED` | Email | Welcome email |
| `PAYMENT_FAILED` | Slack | Payment failure alert |
| `ORDER_SHIPPED` | SMS | Shipment tracking |
| `SECURITY_ALERT` | Email + Slack | Security notification |

## Project Structure (Clean Architecture)

```
notification-service-dotnet/
├── src/
│   ├── NotificationService.Domain/        # Domain layer
│   │   ├── Entities/                      # Notification, NotificationRule
│   │   ├── ValueObjects/                  # Channel, NotificationStatus
│   │   └── Enums/                         # EventType, ChannelType
│   │
│   ├── NotificationService.Application/   # Application layer
│   │   ├── UseCases/                      # ProcessEventUseCase
│   │   └── Interfaces/                    # INotificationSender, IRuleEngine
│   │
│   ├── NotificationService.Infrastructure/ # Infrastructure layer
│   │   ├── Messaging/                     # KafkaConsumer
│   │   ├── Providers/                     # EmailProvider, SlackProvider, SmsProvider
│   │   └── Persistence/                   # NotificationRepository
│   │
│   └── NotificationService.Worker/        # Host / entry point
│       └── Program.cs
│
├── tests/                                 # Unit & integration tests
├── Dockerfile                             # Container image
└── README.md                              # This file
```

## Dependency Rule

```
Infrastructure → Application → Domain
```

The Domain layer has **zero dependencies** on external frameworks. The Application layer depends only on Domain. Infrastructure implements the interfaces defined in Application.

## Processing Flow

```
Event consumed from Kafka
        ↓
ProcessEventUseCase.ExecuteAsync(event)
        ↓
IRuleEngine.Evaluate(event) → NotificationRule
        ↓
INotificationSender.SendAsync(notification)
        ↓
Notification persisted with status (SENT / FAILED)
```

## Status

🔲 Not yet implemented — this service will be created as part of the incremental development process.
