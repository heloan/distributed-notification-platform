# API Gateway (.NET 8)

> Minimal API entry point for the Distributed Smart Notification Platform.

## Responsibilities

- Accept external HTTP requests
- Validate request input (FluentValidation)
- Route requests to downstream services
- Provide health check and readiness endpoints
- Global exception handling with consistent error responses
- Structured logging with Serilog
- Resilience policies (retry + circuit breaker via Polly)
- Observability (OpenTelemetry + Prometheus)
- Authentication-ready structure

## Tech Stack

| Technology | Purpose |
|-----------|---------|
| .NET 8 | Runtime |
| Minimal API | Lightweight HTTP layer |
| HttpClientFactory | Resilient service calls |
| Polly | Retry + Circuit Breaker policies |
| FluentValidation | Request validation |
| Serilog | Structured logging |
| OpenTelemetry | Distributed tracing & metrics |
| Prometheus | Metrics exporter |
| Swagger / OpenAPI | API documentation |
| xUnit + Moq + FluentAssertions | Testing |

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/events` | Submit a new event |
| `GET` | `/events` | List processed events |
| `GET` | `/events/{id}` | Retrieve a single event |
| `GET` | `/health` | Liveness check |
| `GET` | `/health/ready` | Readiness check (includes downstream health) |
| `GET` | `/metrics` | Prometheus metrics |
| `GET` | `/swagger` | API documentation (Development) |

## Project Structure

```
api-gateway-dotnet/
├── src/
│   ├── Gateway.Api/                    # Minimal API host
│   │   ├── Endpoints/                  # Route definitions
│   │   │   ├── EventEndpoints.cs       # Event CRUD routes
│   │   │   └── HealthEndpoints.cs      # Health & readiness
│   │   ├── Middleware/                 # Cross-cutting concerns
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Extensions/                # Service registration
│   │   │   ├── ApplicationServiceExtensions.cs
│   │   │   ├── OpenTelemetryExtensions.cs
│   │   │   └── SwaggerExtensions.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── Program.cs                 # Application entry point
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   │
│   ├── Gateway.Application/           # Application layer
│   │   ├── DTOs/                      # Data transfer objects
│   │   │   ├── EventRequest.cs
│   │   │   ├── EventResponse.cs
│   │   │   └── ErrorResponse.cs
│   │   ├── Interfaces/                # Service contracts
│   │   │   └── IEventService.cs
│   │   └── Validators/                # FluentValidation rules
│   │       └── EventRequestValidator.cs
│   │
│   └── Gateway.Infrastructure/        # Infrastructure layer
│       ├── Configuration/             # Settings binding
│       │   └── EventServiceSettings.cs
│       ├── HttpClients/               # HTTP client implementations
│       │   └── EventServiceClient.cs
│       └── DependencyInjection.cs     # Infrastructure DI registration
│
├── tests/
│   └── Gateway.Api.Tests/             # Unit & integration tests
│       ├── Validators/
│       │   └── EventRequestValidatorTests.cs
│       ├── HttpClients/
│       │   └── EventServiceClientTests.cs
│       ├── Endpoints/
│       │   └── EventEndpointTests.cs
│       └── Middleware/
│           └── ExceptionHandlingMiddlewareTests.cs
│
├── ApiGateway.sln                     # Solution file
├── Dockerfile                         # Multi-stage Docker build
├── .dockerignore
└── README.md                          # This file
```

## Architecture

```
Infrastructure → Application → Domain
```

- **Application layer** has zero external dependencies (only FluentValidation)
- **Infrastructure layer** implements interfaces defined in Application
- **API layer** wires everything together via dependency injection

## Resilience Policies

| Policy | Configuration |
|--------|--------------|
| **Retry** | Exponential backoff, 3 attempts |
| **Circuit Breaker** | Opens after 5 failures, 30s recovery |

## Run Locally

```bash
cd services/api-gateway-dotnet
dotnet run --project src/Gateway.Api
```

## Run Tests

```bash
cd services/api-gateway-dotnet
dotnet test
```

## Status

✅ Implemented
