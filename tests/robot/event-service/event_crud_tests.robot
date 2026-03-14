*** Settings ***
Documentation    Event Service — CRUD tests.
Library          RequestsLibrary
Library          Collections
Resource         ../resources/common.resource
Suite Setup      Create Event Service Session

*** Test Cases ***
Create Event Returns 201
    [Documentation]    POST /api/events should return 201 Created.
    [Tags]             event_service    crud
    ${payload}=        Build Event Payload    USER_REGISTERED    es-robot-001    es@robot.com
    ${response}=       POST On Session    event-service    /api/events    json=${payload}    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    201

Get All Events Returns 200
    [Documentation]    GET /api/events should return 200.
    [Tags]             event_service    crud
    ${response}=       GET On Session    event-service    /api/events    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    200

Get Event By Id Returns 200
    [Documentation]    A created event should be retrievable by ID.
    [Tags]             event_service    crud
    ${payload}=        Build Event Payload    ORDER_SHIPPED    es-robot-002    es2@robot.com
    ${create}=         POST On Session    event-service    /api/events    json=${payload}    expected_status=any
    Run Keyword If     ${create.status_code} == 201
    ...                Verify Event Retrieval    ${create}

Get Nonexistent Event Returns 404
    [Documentation]    GET /api/events/{bad-id} should return 404.
    [Tags]             event_service    crud
    ${response}=       GET On Session    event-service    /api/events/00000000-0000-0000-0000-000000000000    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    404

*** Keywords ***
Verify Event Retrieval
    [Arguments]    ${create_response}
    ${body}=       Set Variable    ${create_response.json()}
    ${id}=         Get From Dictionary    ${body}    id
    ${get}=        GET On Session    event-service    /api/events/${id}
    Should Be Equal As Integers    ${get.status_code}    200
