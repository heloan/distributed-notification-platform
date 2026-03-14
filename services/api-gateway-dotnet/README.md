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
│   ├── Gateway.Api.Tests/             # .NET Unit & integration tests (xUnit)
│   │   ├── Validators/
│   │   │   └── EventRequestValidatorTests.cs
│   │   ├── HttpClients/
│   │   │   └── EventServiceClientTests.cs
│   │   ├── Endpoints/
│   │   │   └── EventEndpointTests.cs
│   │   └── Middleware/
│   │       └── ExceptionHandlingMiddlewareTests.cs
│   │
│   ├── integration/                   # Python integration tests (pytest)
│   │   ├── conftest.py                # Shared fixtures & config
│   │   ├── test_health.py             # Health endpoint tests
│   │   ├── test_events_validation.py  # Input validation tests
│   │   ├── test_events_forwarding.py  # Downstream forwarding tests
│   │   ├── test_observability.py      # Metrics & Swagger tests
│   │   ├── test_swagger_ui_selenium.py # Selenium browser tests
│   │   ├── pytest.ini                 # pytest configuration
│   │   └── requirements.txt           # Python dependencies
│   │
│   ├── robot/                         # Robot Framework automated tests
│   │   └── tests/
│   │       ├── health_tests.robot
│   │       ├── event_validation_tests.robot
│   │       ├── event_forwarding_tests.robot
│   │       ├── observability_tests.robot
│   │       └── swagger_ui_tests.robot  # Selenium UI tests
│   │
│   └── manual/
│       └── manual-test-cases.md       # 15 manual test procedures
│
├── scripts/
│   ├── run-tests.sh                   # Run all test suites
│   ├── run-integration-tests.sh       # Python integration only
│   ├── run-robot-tests.sh             # Robot Framework only
│   └── run-selenium-tests.sh          # Selenium browser only
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

## Testing Strategy

The API Gateway has a **multi-layered testing approach** covering unit, integration, API automation, and browser testing.

| Layer | Tool | Test Count | Scope |
|-------|------|-----------|-------|
| **Unit Tests** | xUnit + Moq + FluentAssertions | 10+ | Validators, HTTP clients, middleware, endpoints |
| **Integration Tests** | Python + pytest + requests | 20+ | Live API validation, forwarding, observability |
| **API Automation** | Robot Framework + RequestsLibrary | 20+ | End-to-end API endpoint automation |
| **Browser Tests** | Selenium (Python + Robot) | 8+ | Swagger UI rendering and interaction |
| **Manual Tests** | Documented procedures | 15 | Step-by-step test cases with checklist |

### Run All Tests

```bash
cd services/api-gateway-dotnet
./scripts/run-tests.sh
```

### Run Individual Test Suites

```bash
# .NET Unit Tests (xUnit)
dotnet test

# Python Integration Tests (pytest)
./scripts/run-integration-tests.sh

# Robot Framework API Tests
./scripts/run-robot-tests.sh

# Selenium Browser Tests (Swagger UI)
./scripts/run-selenium-tests.sh
```

### Test Reports

| Suite | Report Location |
|-------|----------------|
| pytest | `tests/integration/reports/pytest-report.html` |
| Robot Framework | `tests/robot/reports/report.html` |
| Selenium | `tests/robot/reports/selenium/report.html` |

### Prerequisites for Tests

- **API Gateway running** on `http://localhost:5000`
- **Python 3.10+** (for integration and Robot tests)
- **Chrome/Chromium** (for Selenium browser tests)

## Status

✅ Implemented
