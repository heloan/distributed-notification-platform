*** Settings ***
Documentation     API Gateway — Observability Automated Tests
...               Validates Prometheus metrics and Swagger documentation.
Library           RequestsLibrary
Library           Collections
Library           String

Suite Setup       Create Session    gateway    ${BASE_URL}    verify=${FALSE}

*** Variables ***
${BASE_URL}       http://localhost:5000

*** Test Cases ***
TC-014: Prometheus Metrics Endpoint Returns 200
    [Documentation]    GET /metrics returns Prometheus-format metrics
    [Tags]    observability    metrics    smoke
    ${response}=    GET On Session    gateway    /metrics
    Status Should Be    200    ${response}
    Should Contain    ${response.headers}[Content-Type]    text/plain
    Should Not Be Empty    ${response.text}

TC-013: Swagger JSON Specification Is Accessible
    [Documentation]    GET /swagger/v1/swagger.json returns API specification
    [Tags]    observability    swagger
    ${response}=    GET On Session    gateway    /swagger/v1/swagger.json
    Status Should Be    200    ${response}
    ${body}=    Set Variable    ${response.json()}
    Should Be Equal As Strings    ${body}[info][title]    DSNP — API Gateway
    Should Be Equal As Strings    ${body}[info][version]    v1

Swagger Spec Contains Event Endpoints
    [Documentation]    Swagger spec lists /events and /health paths
    [Tags]    observability    swagger
    ${response}=    GET On Session    gateway    /swagger/v1/swagger.json
    ${body}=    Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}[paths]    /events
    Dictionary Should Contain Key    ${body}[paths]    /health

Swagger UI HTML Page Loads
    [Documentation]    Swagger UI is accessible at /swagger/index.html
    [Tags]    observability    swagger
    ${response}=    GET On Session    gateway    /swagger/index.html
    Status Should Be    200    ${response}
    Should Contain    ${response.headers}[Content-Type]    text/html
