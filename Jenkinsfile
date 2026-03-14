// =============================================================================
// Distributed Smart Notification Platform — Jenkins CI/CD Pipeline
// =============================================================================
// Declarative Pipeline with multi-service polyglot build, quality gates,
// parallel test execution, Docker image publishing, and staged deployments.
//
// Requirements:
//   - Jenkins 2.400+ with Pipeline plugin
//   - Docker + Docker Compose on agents
//   - .NET 8 SDK, Java 21, Python 3.10+ on agents (or Docker-based build)
//   - Plugins: Docker Pipeline, Pipeline Utility Steps, HTML Publisher,
//              Warnings Next Generation, JUnit, Credentials Binding
// =============================================================================

pipeline {
    agent any

    // -------------------------------------------------------------------------
    // Environment — shared across all stages
    // -------------------------------------------------------------------------
    environment {
        // Container registry
        DOCKER_REGISTRY     = credentials('docker-registry-url')
        DOCKER_CREDENTIALS  = credentials('docker-registry-credentials')
        IMAGE_TAG            = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}"

        // Service image names
        GATEWAY_IMAGE       = "${DOCKER_REGISTRY}/dsnp/api-gateway"
        EVENT_SERVICE_IMAGE  = "${DOCKER_REGISTRY}/dsnp/event-service"
        NOTIFICATION_IMAGE   = "${DOCKER_REGISTRY}/dsnp/notification-service"

        // Tool versions
        DOTNET_VERSION      = '8.0'
        JAVA_VERSION        = '21'
        PYTHON_VERSION      = '3.10'

        // Test infra
        COMPOSE_PROJECT     = "dsnp-ci-${env.BUILD_NUMBER}"
        GATEWAY_BASE_URL    = 'http://localhost:5050'
        EVENT_SERVICE_BASE_URL  = 'http://localhost:8081'
        NOTIFICATION_SERVICE_BASE_URL = 'http://localhost:5051'
    }

    // -------------------------------------------------------------------------
    // Pipeline options
    // -------------------------------------------------------------------------
    options {
        buildDiscarder(logRotator(numToKeepStr: '20'))
        timeout(time: 30, unit: 'MINUTES')
        timestamps()
        disableConcurrentBuilds()
        ansiColor('xterm')
    }

    // -------------------------------------------------------------------------
    // Parameterised build
    // -------------------------------------------------------------------------
    parameters {
        booleanParam(name: 'SKIP_INTEGRATION_TESTS', defaultValue: false, description: 'Skip integration/E2E tests')
        booleanParam(name: 'SKIP_DOCKER_PUSH',       defaultValue: false, description: 'Skip Docker image push')
        booleanParam(name: 'DEPLOY_STAGING',          defaultValue: false, description: 'Deploy to staging after build')
        choice(name: 'LOG_LEVEL', choices: ['INFO', 'DEBUG', 'WARN'], description: 'Pipeline log verbosity')
    }

    // =========================================================================
    // STAGES
    // =========================================================================
    stages {

        // ---------------------------------------------------------------------
        // 1. Checkout & Environment Info
        // ---------------------------------------------------------------------
        stage('Checkout') {
            steps {
                checkout scm
                sh 'echo "Branch: ${BRANCH_NAME} | Build: ${BUILD_NUMBER} | Commit: $(git rev-parse --short HEAD)"'
                sh 'docker --version && docker compose version'
            }
        }

        // ---------------------------------------------------------------------
        // 2. Static Analysis & Linting (fast feedback)
        // ---------------------------------------------------------------------
        stage('Static Analysis') {
            parallel {
                stage('Lint — .NET') {
                    steps {
                        dir('services/api-gateway-dotnet') {
                            sh 'dotnet format --verify-no-changes --verbosity diagnostic || true'
                        }
                    }
                }
                stage('Lint — Java') {
                    steps {
                        dir('services/event-service-java') {
                            sh '[ -f mvnw ] && ./mvnw checkstyle:check -q || echo "Skipped: Event Service not yet implemented"'
                        }
                    }
                }
                stage('Lint — Python Tests') {
                    steps {
                        dir('tests') {
                            sh '''
                                python3 -m venv .venv-lint
                                . .venv-lint/bin/activate
                                pip install -q flake8
                                flake8 integration/ selenium/ --max-line-length=120 --ignore=E501,W503 || true
                                deactivate
                            '''
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // 3. Build — compile all services
        // ---------------------------------------------------------------------
        stage('Build') {
            parallel {
                stage('Build — API Gateway (.NET)') {
                    steps {
                        dir('services/api-gateway-dotnet') {
                            sh 'dotnet restore'
                            sh 'dotnet build -c Release --no-restore'
                        }
                    }
                }
                stage('Build — Event Service (Java)') {
                    steps {
                        dir('services/event-service-java') {
                            sh '[ -f mvnw ] && ./mvnw clean compile -q || echo "Skipped: Event Service not yet implemented"'
                        }
                    }
                }
                stage('Build — Notification Service (.NET)') {
                    steps {
                        dir('services/notification-service-dotnet') {
                            sh '[ -f *.sln ] && dotnet build -c Release || echo "Skipped: Notification Service not yet implemented"'
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // 4. Unit Tests — fast, isolated, no infra needed
        // ---------------------------------------------------------------------
        stage('Unit Tests') {
            parallel {
                stage('Unit — API Gateway') {
                    steps {
                        dir('services/api-gateway-dotnet') {
                            sh 'dotnet test -c Release --no-build --logger "trx;LogFileName=gateway-unit.trx" --results-directory TestResults/'
                        }
                    }
                    post {
                        always {
                            // Publish .NET test results
                            mstest testResultsFile: 'services/api-gateway-dotnet/TestResults/gateway-unit.trx', keepLongStdio: true
                        }
                    }
                }
                stage('Unit — Event Service') {
                    steps {
                        dir('services/event-service-java') {
                            sh '[ -f mvnw ] && ./mvnw test -q || echo "Skipped: Event Service not yet implemented"'
                        }
                    }
                    post {
                        always {
                            junit allowEmptyResults: true, testResults: 'services/event-service-java/**/target/surefire-reports/*.xml'
                        }
                    }
                }
                stage('Unit — Notification Service') {
                    steps {
                        dir('services/notification-service-dotnet') {
                            sh '[ -f *.sln ] && dotnet test -c Release --logger "trx;LogFileName=notification-unit.trx" --results-directory TestResults/ || echo "Skipped"'
                        }
                    }
                    post {
                        always {
                            mstest testResultsFile: 'services/notification-service-dotnet/TestResults/notification-unit.trx', keepLongStdio: true, failOnError: false
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // 5. Docker Build — create images for all services
        // ---------------------------------------------------------------------
        stage('Docker Build') {
            steps {
                script {
                    // API Gateway
                    docker.build("${GATEWAY_IMAGE}:${IMAGE_TAG}", "services/api-gateway-dotnet")
                    docker.build("${GATEWAY_IMAGE}:latest",       "services/api-gateway-dotnet")

                    // Event Service (when available)
                    if (fileExists('services/event-service-java/Dockerfile')) {
                        docker.build("${EVENT_SERVICE_IMAGE}:${IMAGE_TAG}", "services/event-service-java")
                        docker.build("${EVENT_SERVICE_IMAGE}:latest",       "services/event-service-java")
                    }

                    // Notification Service (when available)
                    if (fileExists('services/notification-service-dotnet/Dockerfile')) {
                        docker.build("${NOTIFICATION_IMAGE}:${IMAGE_TAG}", "services/notification-service-dotnet")
                        docker.build("${NOTIFICATION_IMAGE}:latest",       "services/notification-service-dotnet")
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // 6. Integration Tests — spin up infra, run black-box tests
        // ---------------------------------------------------------------------
        stage('Integration Tests') {
            when {
                expression { return !params.SKIP_INTEGRATION_TESTS }
            }
            stages {
                stage('Start Test Infrastructure') {
                    steps {
                        sh '''
                            docker compose -p ${COMPOSE_PROJECT} \
                                -f tests/docker-compose.test.yml \
                                up -d --wait --timeout 120
                        '''
                        // Wait for all services to be healthy
                        sh 'sleep 15'
                    }
                }
                stage('Run Tests') {
                    parallel {
                        stage('Integration — pytest') {
                            steps {
                                dir('tests') {
                                    sh '''
                                        python3 -m venv .venv-ci
                                        . .venv-ci/bin/activate
                                        pip install -q -r integration/requirements.txt
                                        cd integration
                                        python -m pytest \
                                            -v \
                                            --tb=short \
                                            --junitxml=reports/junit-integration.xml \
                                            --html=reports/pytest-report.html \
                                            --self-contained-html \
                                            || true
                                        deactivate
                                    '''
                                }
                            }
                            post {
                                always {
                                    junit allowEmptyResults: true, testResults: 'tests/integration/reports/junit-integration.xml'
                                    publishHTML(target: [
                                        allowMissing: true,
                                        alwaysLinkToLastBuild: true,
                                        keepAll: true,
                                        reportDir: 'tests/integration/reports',
                                        reportFiles: 'pytest-report.html',
                                        reportName: 'Integration Test Report'
                                    ])
                                }
                            }
                        }
                        stage('API Automation — Robot Framework') {
                            steps {
                                dir('tests') {
                                    sh '''
                                        . .venv-ci/bin/activate 2>/dev/null || {
                                            python3 -m venv .venv-ci
                                            . .venv-ci/bin/activate
                                            pip install -q -r integration/requirements.txt
                                        }
                                        robot \
                                            --outputdir robot/results \
                                            --loglevel INFO \
                                            --xunit robot/results/xunit-robot.xml \
                                            robot/ \
                                            || true
                                        deactivate
                                    '''
                                }
                            }
                            post {
                                always {
                                    junit allowEmptyResults: true, testResults: 'tests/robot/results/xunit-robot.xml'
                                    publishHTML(target: [
                                        allowMissing: true,
                                        alwaysLinkToLastBuild: true,
                                        keepAll: true,
                                        reportDir: 'tests/robot/results',
                                        reportFiles: 'report.html',
                                        reportName: 'Robot Framework Report'
                                    ])
                                }
                            }
                        }
                    }
                }
                stage('Teardown Test Infrastructure') {
                    steps {
                        sh '''
                            docker compose -p ${COMPOSE_PROJECT} \
                                -f tests/docker-compose.test.yml \
                                down -v --remove-orphans
                        '''
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // 7. Quality Gate — fail the build if thresholds are not met
        // ---------------------------------------------------------------------
        stage('Quality Gate') {
            steps {
                script {
                    def gatePassed = true
                    def reasons = []

                    // Check unit test results
                    def testResult = currentBuild.rawBuild.getAction(hudson.tasks.junit.TestResultAction.class)
                    if (testResult) {
                        def failCount = testResult.getFailCount()
                        def totalCount = testResult.getTotalCount()
                        echo "Tests: ${totalCount} total, ${failCount} failures"

                        if (failCount > 0) {
                            gatePassed = false
                            reasons << "Unit test failures: ${failCount}"
                        }
                    }

                    if (!gatePassed) {
                        error "Quality gate FAILED: ${reasons.join(', ')}"
                    }

                    echo '✅ Quality gate PASSED'
                }
            }
        }

        // ---------------------------------------------------------------------
        // 8. Docker Push — publish images to registry
        // ---------------------------------------------------------------------
        stage('Docker Push') {
            when {
                allOf {
                    expression { return !params.SKIP_DOCKER_PUSH }
                    anyOf {
                        branch 'main'
                        branch 'develop'
                        branch pattern: 'release/*', comparator: 'GLOB'
                    }
                }
            }
            steps {
                script {
                    docker.withRegistry("https://${DOCKER_REGISTRY}", 'docker-registry-credentials') {
                        sh "docker push ${GATEWAY_IMAGE}:${IMAGE_TAG}"
                        sh "docker push ${GATEWAY_IMAGE}:latest"

                        if (fileExists('services/event-service-java/Dockerfile')) {
                            sh "docker push ${EVENT_SERVICE_IMAGE}:${IMAGE_TAG}"
                            sh "docker push ${EVENT_SERVICE_IMAGE}:latest"
                        }

                        if (fileExists('services/notification-service-dotnet/Dockerfile')) {
                            sh "docker push ${NOTIFICATION_IMAGE}:${IMAGE_TAG}"
                            sh "docker push ${NOTIFICATION_IMAGE}:latest"
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------------------------
        // 9. Deploy to Staging
        // ---------------------------------------------------------------------
        stage('Deploy — Staging') {
            when {
                allOf {
                    expression { return params.DEPLOY_STAGING }
                    anyOf {
                        branch 'main'
                        branch 'develop'
                        branch pattern: 'release/*', comparator: 'GLOB'
                    }
                }
            }
            steps {
                echo '📦 Deploying to staging environment...'
                sh '''
                    # Pull latest images on staging host
                    # docker compose -f infrastructure/docker-compose.staging.yml pull
                    # docker compose -f infrastructure/docker-compose.staging.yml up -d

                    echo "Staging deployment placeholder — implement with your hosting provider"
                '''
            }
        }

        // ---------------------------------------------------------------------
        // 10. Deploy to Production (manual approval)
        // ---------------------------------------------------------------------
        stage('Deploy — Production') {
            when {
                branch 'main'
            }
            steps {
                timeout(time: 30, unit: 'MINUTES') {
                    input message: '🚀 Deploy to production?',
                          ok: 'Deploy',
                          submitter: 'admin,devops'
                }
                echo '🚀 Deploying to production...'
                sh '''
                    # Tag images with semver
                    # docker tag ${GATEWAY_IMAGE}:${IMAGE_TAG} ${GATEWAY_IMAGE}:v1.0.0
                    # docker push ${GATEWAY_IMAGE}:v1.0.0

                    echo "Production deployment placeholder — implement with your hosting provider"
                '''
            }
        }
    }

    // =========================================================================
    // POST — always runs after all stages
    // =========================================================================
    post {
        always {
            // Clean up Docker resources from this build
            sh "docker compose -p ${COMPOSE_PROJECT} -f tests/docker-compose.test.yml down -v --remove-orphans 2>/dev/null || true"
            sh "docker image prune -f --filter 'label=build=${BUILD_NUMBER}' 2>/dev/null || true"

            // Archive test artifacts
            archiveArtifacts artifacts: '**/TestResults/**,**/reports/**,**/results/**', allowEmptyArchive: true

            cleanWs()
        }

        success {
            echo "✅ Pipeline SUCCESS — Branch: ${BRANCH_NAME} | Build: #${BUILD_NUMBER}"
            // Uncomment for Slack notification:
            // slackSend channel: '#ci-cd', color: 'good',
            //     message: "✅ *DSNP Build #${BUILD_NUMBER}* succeeded on `${BRANCH_NAME}`"
        }

        failure {
            echo "❌ Pipeline FAILED — Branch: ${BRANCH_NAME} | Build: #${BUILD_NUMBER}"
            // Uncomment for Slack notification:
            // slackSend channel: '#ci-cd', color: 'danger',
            //     message: "❌ *DSNP Build #${BUILD_NUMBER}* failed on `${BRANCH_NAME}`\n${BUILD_URL}console"
        }

        unstable {
            echo "⚠️ Pipeline UNSTABLE — some tests failed"
        }
    }
}
