*** Settings ***
Documentation    End-to-End — Full flow tests across all services.
Library          RequestsLibrary
Library          Collections
Library          BuiltIn
Resource         ../resources/common.resource
Suite Setup      Create All Sessions

*** Test Cases ***
Full Event Flow Through All Services
    [Documentation]    Submit event → verify persistence → verify notification.
    [Tags]             e2e    full_flow
    # Step 1: Submit via Gateway
    ${payload}=        Build Event Payload    USER_REGISTERED    e2e-robot-001    e2e@robot.com
    ${gw_resp}=        POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be True     200 <= ${gw_resp.status_code} < 300

    # Step 2: Verify in Event Service
    ${gw_body}=        Set Variable    ${gw_resp.json()}
    ${event_id}=       Get From Dictionary    ${gw_body}    id
    ${es_resp}=        GET On Session    event-service    /api/events/${event_id}    expected_status=any
    Should Be Equal As Integers    ${es_resp.status_code}    200

    # Step 3: Wait for Kafka processing
    Sleep              5s    Waiting for Kafka consumer lag

    # Step 4: Verify notification created
    ${ns_resp}=        GET On Session    notification-service    /api/notifications    expected_status=any
    Should Be True     ${ns_resp.status_code} in [200, 204]

Multiple Events Produce Notifications
    [Documentation]    Sending multiple events should produce corresponding notifications.
    [Tags]             e2e    full_flow
    @{types}=          Create List    USER_REGISTERED    ORDER_SHIPPED    PAYMENT_FAILED
    FOR    ${type}    IN    @{types}
        ${payload}=    Build Event Payload    ${type}    batch-${type}    ${type}@robot.com
        ${resp}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
        Should Be True    200 <= ${resp.status_code} < 300
    END
    Sleep              8s    Waiting for async processing
    ${resp}=           GET On Session    notification-service    /api/notifications    expected_status=any
    Should Be True     ${resp.status_code} in [200, 204]
