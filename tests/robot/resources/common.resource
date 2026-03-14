*** Settings ***
Documentation    Shared resources for all Robot Framework tests.
Library          RequestsLibrary
Library          Collections
Library          OperatingSystem
Resource         variables.py

*** Variables ***
${GATEWAY_URL}           http://localhost:5000
${EVENT_SERVICE_URL}     http://localhost:8080
${NOTIFICATION_URL}      http://localhost:5001
${CONTENT_TYPE}          application/json

*** Keywords ***
Create Gateway Session
    [Documentation]    Create a reusable HTTP session to the API Gateway.
    Create Session     gateway    ${GATEWAY_URL}    verify=${False}

Create Event Service Session
    [Documentation]    Create a reusable HTTP session to the Event Service.
    Create Session     event-service    ${EVENT_SERVICE_URL}    verify=${False}

Create Notification Service Session
    [Documentation]    Create a reusable HTTP session to the Notification Service.
    Create Session     notification-service    ${NOTIFICATION_URL}    verify=${False}

Create All Sessions
    [Documentation]    Create sessions for all services.
    Create Gateway Session
    Create Event Service Session
    Create Notification Service Session

Build Event Payload
    [Documentation]    Build a valid event payload.
    [Arguments]        ${event_type}    ${user_id}=test-user-001    ${email}=test@email.com
    ${payload}=        Create Dictionary    eventType=${event_type}    userId=${user_id}    email=${email}
    RETURN             ${payload}

Response Status Should Be
    [Documentation]    Assert the response status code.
    [Arguments]        ${response}    ${expected_status}
    Should Be Equal As Integers    ${response.status_code}    ${expected_status}

Response Should Contain Key
    [Documentation]    Assert the JSON response contains a key.
    [Arguments]        ${response}    ${key}
    ${body}=           Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}    ${key}
