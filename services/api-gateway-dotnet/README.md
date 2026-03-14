# API Gateway (.NET 8)

> Minimal API entry point for the Distributed Smart Notification Platform.

## Responsibilities

- Accept external HTTP requests
- Validate request input
- Route requests to downstream services
- Provide health check endpoint
- Authentication-ready structure

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| .NET 8 | Runtime |
| Minimal API | Lightweight HTTP layer |
| HttpClientFactory | Resilient service calls |
| Serilog | Structured logging |
| OpenTelemetry | Observability |

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/events` | Submit a new event |
| `GET` | `/events` | List processed events |
| `GET` | `/health` | Health check |

## Project Structure

```
api-gateway-dotnet/
├── src/
│   ├── Gateway.Api/           # Minimal API host
│   │   ├── Endpoints/         # Route definitions
│   │   ├── Middleware/        # Cross-cutting concerns
│   │   ├── Extensions/       # Service registration
│   │   └── Program.cs        # Application entry point
│   │
│   ├── Gateway.Application/   # Application layer
│   │   ├── DTOs/             # Data transfer objects
│   │   ├── Interfaces/       # Service contracts
│   │   └── Services/         # Application services
│   │
│   └── Gateway.Infrastructure/ # Infrastructure layer
│       ├── HttpClients/       # HTTP client implementations
│       └── Configuration/     # Configuration bindings
│
├── tests/                     # Unit & integration tests
├── Dockerfile                 # Container image
└── README.md                  # This file
```

## Status

🔲 Not yet implemented — this service will be created as part of the incremental development process.
