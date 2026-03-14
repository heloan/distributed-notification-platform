# 🧪 Test Suite — Distributed Smart Notification Platform

> Centralised test suite covering all services with multiple test layers.

## 📁 Structure

```
tests/
├── integration/                # Python + pytest — HTTP-level black-box tests
│   ├── conftest.py             # Shared fixtures & service URLs
│   ├── pytest.ini              # pytest configuration & markers
│   ├── requirements.txt        # Python dependencies
│   ├── gateway/                # API Gateway integration tests
│   ├── event-service/          # Event Service integration tests
│   ├── notification-service/   # Notification Service integration tests
│   └── end-to-end/             # Full-pipeline E2E tests
├── robot/                      # Robot Framework — keyword-driven API tests
│   ├── resources/              # Shared keywords & variables
│   ├── gateway/                # Gateway .robot files
│   ├── event-service/          # Event Service .robot files
│   └── end-to-end/             # E2E .robot files
├── selenium/                   # Selenium — browser UI tests
│   ├── conftest.py             # WebDriver management
│   ├── test_swagger_ui.py      # Swagger UI verification
│   └── test_grafana_dashboard.py # Grafana dashboard verification
├── manual/                     # Manual test case documentation
│   ├── gateway-test-cases.md
│   ├── event-service-test-cases.md
│   └── end-to-end-test-cases.md
├── scripts/                    # Shell scripts to run test suites
│   ├── run-all-tests.sh
│   ├── run-integration-tests.sh
│   ├── run-robot-tests.sh
│   ├── run-selenium-tests.sh
│   └── run-e2e-tests.sh
└── docker-compose.test.yml     # Isolated test infrastructure
```

## 🏗️ Test Pyramid

| Layer | Tool | Scope | Location |
|-------|------|-------|----------|
| **Unit** | xUnit / JUnit | Single class/method | Inside each service (`services/*/tests/`) |
| **Integration** | pytest + requests | Single service HTTP API | `tests/integration/{service}/` |
| **End-to-End** | pytest + requests | Full pipeline (Gateway → Kafka → Notification) | `tests/integration/end-to-end/` |
| **API Automation** | Robot Framework | Keyword-driven API validation | `tests/robot/` |
| **Browser** | Selenium + pytest | Swagger UI, Grafana dashboards | `tests/selenium/` |
| **Manual** | Markdown docs | Exploratory & edge cases | `tests/manual/` |

## 🚀 Quick Start

### Prerequisites
- Python 3.10+
- Services running (via Docker Compose)
- Chrome/Chromium (for Selenium tests)

### Run all tests
```bash
chmod +x tests/scripts/*.sh
./tests/scripts/run-all-tests.sh
```

### Run specific suites
```bash
# Integration tests only
./tests/scripts/run-integration-tests.sh

# Only gateway integration tests
./tests/scripts/run-integration-tests.sh -m gateway

# Only end-to-end tests
./tests/scripts/run-e2e-tests.sh

# Robot Framework tests
./tests/scripts/run-robot-tests.sh

# Robot — only gateway tag
./tests/scripts/run-robot-tests.sh --include gateway

# Selenium tests
./tests/scripts/run-selenium-tests.sh

# Selenium — only swagger
./tests/scripts/run-selenium-tests.sh -k swagger
```

### Test infrastructure
```bash
# Start isolated test environment
docker compose -f tests/docker-compose.test.yml up -d

# Run tests against it
GATEWAY_BASE_URL=http://localhost:5050 \
EVENT_SERVICE_BASE_URL=http://localhost:8081 \
./tests/scripts/run-integration-tests.sh

# Tear down
docker compose -f tests/docker-compose.test.yml down -v
```

## 📊 Reports

| Suite | Report Location |
|-------|-----------------|
| Integration (pytest) | `tests/integration/reports/pytest-report.html` |
| Robot Framework | `tests/robot/results/report.html` |
| Selenium | `tests/selenium/reports/selenium-report.html` |

## 🏷️ Test Markers / Tags

### pytest markers
- `smoke` — Quick sanity checks
- `health` — Health endpoint tests
- `validation` — Input validation
- `forwarding` — Event forwarding
- `observability` — Metrics & Swagger
- `gateway` — API Gateway scope
- `event_service` — Event Service scope
- `notification_service` — Notification Service scope
- `e2e` — End-to-end flow

### Robot Framework tags
- `smoke`, `health`, `gateway`, `event_service`, `e2e`, `full_flow`, `crud`, `validation`, `forwarding`

## 🧱 Architecture Decision

> **Unit tests** live inside each service (`services/*/tests/`) because they are tightly coupled to internal implementation and run during CI build.  
> **Black-box tests** (integration, robot, selenium, manual) live here in root `/tests/` because they test services as external consumers through HTTP, independent of implementation language or framework.
