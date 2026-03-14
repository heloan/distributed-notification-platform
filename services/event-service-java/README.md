# Event Service (Java 21 / Spring Boot)

> Event ingestion and publishing service for the Distributed Smart Notification Platform.

## Responsibilities

- Receive application events via REST API
- Validate event data
- Persist events to PostgreSQL
- Publish domain events to Apache Kafka

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| Java 21 | Runtime |
| Spring Boot 3.x | Framework |
| Spring Data JPA | Data access |
| PostgreSQL | Event storage |
| Apache Kafka | Event publishing |
| OpenTelemetry | Observability |

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/events` | Create and publish an event |
| `GET` | `/events` | Retrieve all events |
| `GET` | `/events/{id}` | Retrieve a single event |
| `GET` | `/actuator/health` | Health check |

## Event Payload

```json
{
  "eventType": "USER_REGISTERED",
  "userId": "123",
  "email": "user@email.com",
  "timestamp": "2026-03-14T10:15:00"
}
```

## Project Structure

```
event-service-java/
├── src/
│   └── main/
│       ├── java/com/dsnp/eventservice/
│       │   ├── controller/       # REST controllers
│       │   ├── service/          # Business logic
│       │   ├── repository/       # Data access
│       │   ├── model/            # Domain entities
│       │   ├── dto/              # Data transfer objects
│       │   ├── config/           # Configuration classes
│       │   └── EventServiceApplication.java
│       │
│       └── resources/
│           └── application.yml   # Application config
│
├── tests/                        # Unit & integration tests
├── pom.xml                       # Maven build
├── Dockerfile                    # Container image
└── README.md                     # This file
```

## Status

🔲 Not yet implemented — this service will be created as part of the incremental development process.
