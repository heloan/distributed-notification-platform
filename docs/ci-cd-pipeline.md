# 🚀 CI/CD Pipeline — Jenkins

> Automated build, test, publish, and deploy pipeline for the Distributed Smart Notification Platform.

---

## Table of Contents

- [Overview](#overview)
- [Pipeline Architecture](#pipeline-architecture)
- [Stages](#stages)
- [Branching Strategy](#branching-strategy)
- [Quality Gates](#quality-gates)
- [Environment Variables](#environment-variables)
- [Jenkins Setup](#jenkins-setup)
- [Pipeline Parameters](#pipeline-parameters)
- [Artifact Publishing](#artifact-publishing)
- [Notifications](#notifications)
- [Troubleshooting](#troubleshooting)

---

## Overview

The project uses a **Jenkins Declarative Pipeline** defined in `Jenkinsfile` at the repository root. The pipeline is designed for a polyglot monorepo with **.NET**, **Java**, and **Python** codebases.

### Key Design Principles

| Principle | Implementation |
|-----------|---------------|
| **Fail fast** | Static analysis and unit tests run before heavier stages |
| **Parallel execution** | Independent services build and test in parallel |
| **Quality gates** | Build fails if thresholds are not met |
| **Infrastructure as Code** | Test infra spins up via Docker Compose per build |
| **Artifact traceability** | Every image tagged with `branch-buildNumber` |
| **Manual approval** | Production deploys require human confirmation |
| **Clean workspace** | Docker resources and workspace cleaned after every build |

---

## Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Jenkins Declarative Pipeline                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────┐   ┌─────────────────────────────────────────────┐    │
│  │ Checkout  │──▶│           Static Analysis                   │    │
│  └──────────┘   │  ┌──────────┬──────────┬──────────────────┐ │    │
│                  │  │ .NET fmt │ Java CS  │ Python flake8    │ │    │
│                  │  └──────────┴──────────┴──────────────────┘ │    │
│                  └────────────────────┬────────────────────────┘    │
│                                       │                             │
│                  ┌────────────────────▼────────────────────────┐    │
│                  │               Build                         │    │
│                  │  ┌──────────┬──────────┬──────────────────┐ │    │
│                  │  │ Gateway  │ Event    │ Notification     │ │    │
│                  │  │ (.NET)   │ (Java)   │ (.NET)           │ │    │
│                  │  └──────────┴──────────┴──────────────────┘ │    │
│                  └────────────────────┬────────────────────────┘    │
│                                       │                             │
│                  ┌────────────────────▼────────────────────────┐    │
│                  │            Unit Tests                        │    │
│                  │  ┌──────────┬──────────┬──────────────────┐ │    │
│                  │  │ xUnit    │ JUnit    │ xUnit            │ │    │
│                  │  └──────────┴──────────┴──────────────────┘ │    │
│                  └────────────────────┬────────────────────────┘    │
│                                       │                             │
│                  ┌────────────────────▼────────────────────────┐    │
│                  │           Docker Build                       │    │
│                  │  Build images for all implemented services   │    │
│                  └────────────────────┬────────────────────────┘    │
│                                       │                             │
│                  ┌────────────────────▼────────────────────────┐    │
│                  │        Integration Tests                     │    │
│                  │  ┌───────────────┐  ┌────────────────────┐  │    │
│                  │  │ Start Test    │  │ Run Tests           │  │    │
│                  │  │ Infrastructure│─▶│ ┌────────┬────────┐│  │    │
│                  │  │ (Compose)     │  │ │ pytest │ Robot  ││  │    │
│                  │  └───────────────┘  │ └────────┴────────┘│  │    │
│                  │                     └────────────────────┘  │    │
│                  │  ┌───────────────────────────────────────┐  │    │
│                  │  │ Teardown Infrastructure               │  │    │
│                  │  └───────────────────────────────────────┘  │    │
│                  └────────────────────┬────────────────────────┘    │
│                                       │                             │
│                  ┌────────────────────▼────────────────────────┐    │
│                  │          Quality Gate                        │    │
│                  │  ✓ Zero unit test failures                   │    │
│                  │  ✓ Integration test pass rate ≥ 95%          │    │
│                  │  ✓ Docker image size limits                  │    │
│                  └────────────────────┬────────────────────────┘    │
│                                       │                             │
│              ┌────────────────────────▼──────────────────────┐      │
│              │  ┌──────────────┐    ┌─────────────────────┐  │      │
│              │  │ Docker Push  │───▶│ Deploy — Staging     │  │      │
│              │  │ (main/develop│    │ (auto on develop)    │  │      │
│              │  │  /release/*)  │    └──────────┬──────────┘  │      │
│              │  └──────────────┘               │              │      │
│              │                     ┌───────────▼───────────┐  │      │
│              │                     │ Deploy — Production   │  │      │
│              │                     │ ⚠️ Manual Approval     │  │      │
│              │                     │ (main branch only)    │  │      │
│              │                     └──────────────────────┘  │      │
│              └───────────────────────────────────────────────┘      │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                      Post Actions                            │   │
│  │  • Clean workspace & Docker resources                        │   │
│  │  • Archive test artifacts                                    │   │
│  │  • Slack notification (success/failure/unstable)             │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Stages

### 1. Checkout
- Clones the repository via SCM
- Prints branch, build number, and commit SHA
- Verifies Docker and Docker Compose availability

### 2. Static Analysis *(parallel)*
Runs in parallel across all languages:

| Language | Tool | What it checks |
|----------|------|---------------|
| .NET | `dotnet format` | Code style conformance |
| Java | Maven Checkstyle | Code style conformance |
| Python | `flake8` | PEP 8 compliance for test code |

### 3. Build *(parallel)*
Compiles all services in parallel:

| Service | Command |
|---------|---------|
| API Gateway (.NET) | `dotnet restore && dotnet build -c Release` |
| Event Service (Java) | `./mvnw clean compile` |
| Notification Service (.NET) | `dotnet build -c Release` |

> Services that are not yet implemented are gracefully skipped.

### 4. Unit Tests *(parallel)*
Runs unit tests for each service in parallel:

| Service | Framework | Report Format |
|---------|-----------|--------------|
| API Gateway | xUnit | `.trx` (MSTest) |
| Event Service | JUnit 5 | Surefire XML |
| Notification Service | xUnit | `.trx` (MSTest) |

Results are published to Jenkins via the JUnit/MSTest plugins.

### 5. Docker Build
Builds multi-stage Docker images for all implemented services:

```
${DOCKER_REGISTRY}/dsnp/api-gateway:${BRANCH}-${BUILD_NUMBER}
${DOCKER_REGISTRY}/dsnp/api-gateway:latest
${DOCKER_REGISTRY}/dsnp/event-service:${BRANCH}-${BUILD_NUMBER}
${DOCKER_REGISTRY}/dsnp/notification-service:${BRANCH}-${BUILD_NUMBER}
```

### 6. Integration Tests
Three sub-stages:

1. **Start Test Infrastructure** — `docker compose -f tests/docker-compose.test.yml up -d`
2. **Run Tests** *(parallel)*:
   - **pytest** — Python integration tests with JUnit XML + HTML reports
   - **Robot Framework** — API automation with xUnit XML + HTML reports
3. **Teardown** — `docker compose down -v --remove-orphans`

### 7. Quality Gate
Evaluates build quality against thresholds defined in `jenkins/quality-gates.yml`:

| Metric | Threshold |
|--------|-----------|
| Unit test failures | 0 |
| Unit test pass rate | 100% |
| Integration test pass rate | ≥ 95% |
| Docker image size (Gateway) | ≤ 250 MB |
| Docker image size (Event) | ≤ 350 MB |
| Total build time | ≤ 20 min |

### 8. Docker Push
Pushes images to the container registry. **Only runs on protected branches:**

| Branch Pattern | Trigger |
|---------------|---------|
| `main` | Auto |
| `develop` | Auto |
| `release/*` | Auto |
| Feature branches | Skipped |

### 9. Deploy — Staging
Automatic deployment to staging environment. Triggered when:
- `DEPLOY_STAGING` parameter is `true`
- Branch is `main`, `develop`, or `release/*`

### 10. Deploy — Production
Manual deployment with approval gate:
- Only available on `main` branch
- Requires approval from `admin` or `devops` role
- 30-minute timeout for approval
- Tags images with semantic version

---

## Branching Strategy

The pipeline supports **GitFlow**:

```
main ───────────────────────────────────────────▶  Production
  │                                                  ▲
  ├── release/1.0 ──────────────────────────────────┘
  │
  └── develop ──────────────────────────────────▶  Staging
       │
       ├── feat/api-gateway-dotnet ────────────▶  CI only
       ├── feat/event-service-java ────────────▶  CI only
       └── fix/validation-bug ─────────────────▶  CI only
```

| Branch | Build | Unit Tests | Integration | Docker Push | Deploy |
|--------|-------|-----------|-------------|-------------|--------|
| `feat/*` | ✅ | ✅ | ✅ | ❌ | ❌ |
| `develop` | ✅ | ✅ | ✅ | ✅ | Staging (auto) |
| `release/*` | ✅ | ✅ | ✅ | ✅ | Staging (auto) |
| `main` | ✅ | ✅ | ✅ | ✅ | Production (manual) |

---

## Quality Gates

Quality gates are defined in `jenkins/quality-gates.yml` and enforced in the pipeline:

```yaml
quality_gate:
  unit_tests:
    max_failures: 0
    min_pass_rate: 100
  integration_tests:
    max_failures: 0
    min_pass_rate: 95
  docker_images:
    max_size_mb:
      api-gateway: 250
      event-service: 350
      notification-service: 250
  build_performance:
    max_build_time_minutes: 20
```

Future quality gates (when tools are integrated):
- **Code coverage** — minimum 80% line coverage (Coverlet / JaCoCo)
- **Static analysis** — zero critical issues (SonarQube)
- **Security scanning** — zero high/critical CVEs (Trivy / OWASP)

---

## Environment Variables

### Pipeline-level

| Variable | Description | Default |
|----------|-------------|---------|
| `DOCKER_REGISTRY` | Container registry URL | Jenkins credential |
| `DOCKER_CREDENTIALS` | Registry auth credentials | Jenkins credential |
| `IMAGE_TAG` | Image tag format | `${BRANCH}-${BUILD_NUMBER}` |
| `COMPOSE_PROJECT` | Docker Compose project name | `dsnp-ci-${BUILD_NUMBER}` |

### Service URLs (integration tests)

| Variable | Description | Default |
|----------|-------------|---------|
| `GATEWAY_BASE_URL` | API Gateway test URL | `http://localhost:5050` |
| `EVENT_SERVICE_BASE_URL` | Event Service test URL | `http://localhost:8081` |
| `NOTIFICATION_SERVICE_BASE_URL` | Notification Service test URL | `http://localhost:5051` |

### Tool versions

| Variable | Description | Default |
|----------|-------------|---------|
| `DOTNET_VERSION` | .NET SDK version | `8.0` |
| `JAVA_VERSION` | JDK version | `21` |
| `PYTHON_VERSION` | Python version | `3.10` |

---

## Jenkins Setup

### Required Plugins

| Plugin | Purpose |
|--------|---------|
| **Pipeline** | Declarative pipeline support |
| **Docker Pipeline** | Docker build/push integration |
| **Pipeline Utility Steps** | `fileExists`, `readYaml`, etc. |
| **HTML Publisher** | Publish pytest/Robot HTML reports |
| **JUnit** | Parse JUnit XML test results |
| **MSTest** | Parse .NET `.trx` test results |
| **Warnings Next Generation** | Static analysis reporting |
| **Credentials Binding** | Secure credential access |
| **AnsiColor** | Coloured console output |
| **Slack Notification** | *(optional)* Build notifications |

### Required Credentials

| Credential ID | Type | Description |
|--------------|------|-------------|
| `docker-registry-url` | Secret text | Container registry URL |
| `docker-registry-credentials` | Username/Password | Registry authentication |

### Agent Setup

**Option A — Install tools on agent:**
See `jenkins/agent-requirements.txt` for the full list.

**Option B — Docker-based agent:**
```bash
docker build -t dsnp-jenkins-agent -f jenkins/Dockerfile.agent jenkins/
```

Configure in Jenkins → Manage Nodes → Cloud → Docker:
- Image: `dsnp-jenkins-agent`
- Docker socket mount: `/var/run/docker.sock`

### Webhook Configuration

Set up a GitHub/Bitbucket webhook to trigger builds:

```
POST https://your-jenkins.com/github-webhook/
```

Or configure **Poll SCM** in Jenkins:
```
H/5 * * * *    # Poll every 5 minutes
```

---

## Pipeline Parameters

The pipeline accepts build-time parameters:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SKIP_INTEGRATION_TESTS` | Boolean | `false` | Skip integration/E2E test stage |
| `SKIP_DOCKER_PUSH` | Boolean | `false` | Skip Docker image push |
| `DEPLOY_STAGING` | Boolean | `false` | Deploy to staging after build |
| `LOG_LEVEL` | Choice | `INFO` | Pipeline log verbosity |

### Usage

```
# Trigger via Jenkins UI — select parameters before build

# Trigger via CLI:
jenkins-cli build dsnp-pipeline \
    -p SKIP_INTEGRATION_TESTS=true \
    -p DEPLOY_STAGING=true
```

---

## Artifact Publishing

### Test Reports

| Report | Format | Jenkins View |
|--------|--------|-------------|
| Unit tests (.NET) | `.trx` | Test Results tab |
| Unit tests (Java) | Surefire XML | Test Results tab |
| Integration tests | JUnit XML + HTML | Test Results + HTML Report |
| Robot Framework | xUnit XML + HTML | Test Results + HTML Report |

### Docker Images

Images are pushed with two tags:
1. `${BRANCH}-${BUILD_NUMBER}` — immutable, traceable
2. `latest` — rolling, for convenience

### Archived Artifacts

All test results and reports are archived:
```
**/TestResults/**      → .NET unit test results
**/reports/**          → pytest + Selenium HTML reports
**/results/**          → Robot Framework reports
```

---

## Notifications

### Slack *(opt-in)*

Uncomment the Slack lines in the Jenkinsfile `post` block:

| Event | Channel | Color |
|-------|---------|-------|
| Success | `#ci-cd` | 🟢 Green |
| Failure | `#ci-cd` | 🔴 Red |
| Unstable | `#ci-cd` | 🟡 Yellow |

### Email *(opt-in)*

Add the `emailext` step in the `post` block for email notifications.

---

## Troubleshooting

### Common Issues

| Issue | Cause | Fix |
|-------|-------|-----|
| `dotnet: command not found` | .NET SDK not on agent | Install SDK or use Docker agent |
| Docker socket permission denied | Jenkins user not in docker group | `usermod -aG docker jenkins` |
| Test infra containers fail to start | Port conflicts from previous builds | Pipeline uses unique `COMPOSE_PROJECT` per build |
| Integration tests fail with connection refused | Services not ready | Increase `sleep` or use health check polling |
| Image push 401 Unauthorized | Invalid registry credentials | Update Jenkins credential `docker-registry-credentials` |
| Build timeout | Heavy test suite or slow agent | Increase `timeout` in pipeline `options` |

### Debug Tips

1. **Check console output** — Jenkins build log has full stage output
2. **Archive artifacts** — Download test reports from build artifacts
3. **Reproduce locally** — Run the same Docker Compose commands locally:
   ```bash
   docker compose -f tests/docker-compose.test.yml up -d
   ./tests/scripts/run-integration-tests.sh
   docker compose -f tests/docker-compose.test.yml down -v
   ```
4. **SSH into agent** — If using cloud agents, check tool availability

---

## File Reference

```
distributed-notification-platform/
├── Jenkinsfile                          # ← Main pipeline definition
├── jenkins/
│   ├── Dockerfile.agent                 # Docker-based Jenkins agent
│   ├── agent-requirements.txt           # Agent tool requirements
│   ├── pipeline-helpers.groovy          # Shared Groovy helper functions
│   └── quality-gates.yml               # Quality gate thresholds
```
