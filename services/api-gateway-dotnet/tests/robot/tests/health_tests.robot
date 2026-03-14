*** Settings ***
Documentation     API Gateway — Health Endpoint Automated Tests
...               Validates health check and readiness endpoints using Robot Framework.
Library           RequestsLibrary
Library           Collections

Suite Setup       Create Session    gateway    ${BASE_URL}    verify=${FALSE}

*** Variables ***
${BASE_URL}       http://localhost:5000

*** Test Cases ***
TC-001: Health Check Returns 200 And Healthy Status
    [Documentation]    GET /health returns 200 with Healthy status
    [Tags]    health    smoke    critical
    ${response}=    GET On Session    gateway    /health
    Status Should Be    200    ${response}
    ${body}=    Set Variable    ${response.json()}
    Should Be Equal As Strings    ${body}[status]    Healthy
    Should Be Equal As Strings    ${body}[service]    API Gateway
    Dictionary Should Contain Key    ${body}    timestamp

TC-002: Readiness Check Returns Valid Response
    [Documentation]    GET /health/ready returns 200 or 503 depending on Event Service
    [Tags]    health    readiness
    ${response}=    GET On Session    gateway    /health/ready    expected_status=any
    Should Be True    ${response.status_code} == 200 or ${response.status_code} == 503
    ${body}=    Set Variable    ${response.json()}
    Should Be Equal As Strings    ${body}[service]    API Gateway

Health Response Content Type Is JSON
    [Documentation]    Health endpoint returns application/json
    [Tags]    health
    ${response}=    GET On Session    gateway    /health
    Should Contain    ${response.headers}[Content-Type]    application/json
