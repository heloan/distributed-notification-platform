*** Settings ***
Documentation     API Gateway — Event Forwarding Automated Tests
...               Validates event forwarding to downstream Event Service.
Library           RequestsLibrary
Library           Collections

Suite Setup       Create Session    gateway    ${BASE_URL}    verify=${FALSE}

*** Variables ***
${BASE_URL}       http://localhost:5000

*** Test Cases ***
TC-003: Submit Valid Event Returns 202 Or 502
    [Documentation]    POST /events with valid payload returns 202 or 502
    [Tags]    forwarding    events    critical
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=test-123
    ...    email=user@email.com
    ...    timestamp=2026-03-14T10:00:00
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=any
    Should Be True    ${response.status_code} == 202 or ${response.status_code} == 502

TC-011: Get All Events Returns 200 Or 502
    [Documentation]    GET /events returns 200 or 502
    [Tags]    forwarding    events
    ${response}=    GET On Session    gateway    /events    expected_status=any
    Should Be True    ${response.status_code} == 200 or ${response.status_code} == 502

TC-012: Get Non-Existent Event Returns 404 Or 502
    [Documentation]    GET /events/{id} with invalid ID returns 404 or 502
    [Tags]    forwarding    events
    ${response}=    GET On Session    gateway    /events/non-existent-id    expected_status=any
    Should Be True    ${response.status_code} == 404 or ${response.status_code} == 502

Bad Gateway Response Has Correct Format
    [Documentation]    502 response follows ErrorResponse schema
    [Tags]    forwarding    error-handling
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=123
    ...    email=user@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=any
    Run Keyword If    ${response.status_code} == 502    Validate Error Response Format    ${response}

*** Keywords ***
Validate Error Response Format
    [Arguments]    ${response}
    ${body}=    Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}    title
    Dictionary Should Contain Key    ${body}    status
    Dictionary Should Contain Key    ${body}    detail
    Should Be Equal As Integers    ${body}[status]    502
