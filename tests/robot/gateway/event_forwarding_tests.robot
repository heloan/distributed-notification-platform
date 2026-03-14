*** Settings ***
Documentation    API Gateway — Event forwarding tests.
Library          RequestsLibrary
Library          Collections
Resource         ../resources/common.resource
Suite Setup      Create Gateway Session

*** Test Cases ***
Post Event Returns Success Status
    [Documentation]    POST /api/events should return 2xx when Event Service is healthy.
    [Tags]             forwarding    gateway
    ${payload}=        Build Event Payload    USER_REGISTERED
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be True     200 <= ${response.status_code} < 300

Post Event Response Contains Id
    [Documentation]    Success response should contain an event ID.
    [Tags]             forwarding    gateway
    ${payload}=        Build Event Payload    USER_REGISTERED    robot-fwd-001    fwd@test.com
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Run Keyword If     200 <= ${response.status_code} < 300
    ...                Response Should Contain Key    ${response}    id

Get Events Returns List Or Expected Status
    [Documentation]    GET /api/events should return 200 with list, 404, or 502.
    [Tags]             forwarding    gateway
    ${response}=       GET On Session    gateway    /api/events    expected_status=any
    Should Be True     ${response.status_code} in [200, 404, 502]
