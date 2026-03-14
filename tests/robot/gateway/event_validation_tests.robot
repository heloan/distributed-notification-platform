*** Settings ***
Documentation    API Gateway — Event validation tests.
Library          RequestsLibrary
Library          Collections
Resource         ../resources/common.resource
Suite Setup      Create Gateway Session

*** Test Cases ***
Valid USER_REGISTERED Event Accepted
    [Documentation]    A well-formed USER_REGISTERED event should not return 400.
    [Tags]             validation    gateway
    ${payload}=        Build Event Payload    USER_REGISTERED
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Not Be Equal As Integers    ${response.status_code}    400

Valid PAYMENT_FAILED Event Accepted
    [Documentation]    A well-formed PAYMENT_FAILED event should not return 400.
    [Tags]             validation    gateway
    ${payload}=        Build Event Payload    PAYMENT_FAILED
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Not Be Equal As Integers    ${response.status_code}    400

Valid ORDER_SHIPPED Event Accepted
    [Documentation]    A well-formed ORDER_SHIPPED event should not return 400.
    [Tags]             validation    gateway
    ${payload}=        Build Event Payload    ORDER_SHIPPED
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Not Be Equal As Integers    ${response.status_code}    400

Valid SECURITY_ALERT Event Accepted
    [Documentation]    A well-formed SECURITY_ALERT event should not return 400.
    [Tags]             validation    gateway
    ${payload}=        Build Event Payload    SECURITY_ALERT
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Not Be Equal As Integers    ${response.status_code}    400

Invalid Event Type Returns 400
    [Documentation]    An unsupported event type should return 400.
    [Tags]             validation    gateway
    ${payload}=        Build Event Payload    UNSUPPORTED_TYPE
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    400

Empty Event Type Returns 400
    [Documentation]    An empty eventType should return 400.
    [Tags]             validation    gateway
    ${payload}=        Create Dictionary    eventType=${EMPTY}    userId=u1    email=u@e.com
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    400

Missing UserId Returns 400
    [Documentation]    Missing userId should return 400.
    [Tags]             validation    gateway
    ${payload}=        Create Dictionary    eventType=USER_REGISTERED    email=u@e.com
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    400

Missing Email Returns 400
    [Documentation]    Missing email should return 400.
    [Tags]             validation    gateway
    ${payload}=        Create Dictionary    eventType=USER_REGISTERED    userId=u1
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    400

Empty Body Returns 400
    [Documentation]    An empty JSON body should return 400.
    [Tags]             validation    gateway
    ${payload}=        Create Dictionary
    ${response}=       POST On Session    gateway    /api/events    json=${payload}    expected_status=any
    Should Be Equal As Integers    ${response.status_code}    400
