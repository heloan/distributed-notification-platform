*** Settings ***
Documentation     API Gateway — Event Validation Automated Tests
...               Validates event request validation rules using Robot Framework.
Library           RequestsLibrary
Library           Collections

Suite Setup       Create Session    gateway    ${BASE_URL}    verify=${FALSE}

*** Variables ***
${BASE_URL}       http://localhost:5000

*** Test Cases ***
TC-005: Invalid Event Type Returns 400
    [Documentation]    POST /events with unsupported event type returns 400
    [Tags]    validation    events    critical
    ${payload}=    Create Dictionary
    ...    eventType=INVALID_TYPE
    ...    userId=123
    ...    email=user@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=400
    ${body}=    Set Variable    ${response.json()}
    Should Be Equal As Strings    ${body}[title]    Validation Failed
    Should Be Equal As Integers    ${body}[status]    400
    Dictionary Should Contain Key    ${body}[errors]    EventType

TC-006: Empty Email Returns 400
    [Documentation]    POST /events with empty email returns 400
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=123
    ...    email=${EMPTY}
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=400
    ${body}=    Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}[errors]    Email

TC-007: Invalid Email Format Returns 400
    [Documentation]    POST /events with invalid email format returns 400
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=123
    ...    email=not-an-email
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=400
    ${body}=    Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}[errors]    Email

TC-008: Empty UserId Returns 400
    [Documentation]    POST /events with empty userId returns 400
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=${EMPTY}
    ...    email=user@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=400
    ${body}=    Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}[errors]    UserId

TC-009: Future Timestamp Returns 400
    [Documentation]    POST /events with future timestamp returns 400
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=123
    ...    email=user@email.com
    ...    timestamp=2030-01-01T00:00:00
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=400
    ${body}=    Set Variable    ${response.json()}
    Dictionary Should Contain Key    ${body}[errors]    Timestamp

TC-010: Multiple Validation Errors Returned
    [Documentation]    POST /events with all fields empty returns all errors at once
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=${EMPTY}
    ...    userId=${EMPTY}
    ...    email=${EMPTY}
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=400
    ${body}=    Set Variable    ${response.json()}
    ${error_count}=    Get Length    ${body}[errors]
    Should Be True    ${error_count} >= 3

Valid Event Type USER_REGISTERED Passes Validation
    [Documentation]    USER_REGISTERED passes validation (202 or 502)
    [Tags]    validation    events    smoke
    ${payload}=    Create Dictionary
    ...    eventType=USER_REGISTERED
    ...    userId=123
    ...    email=user@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=any
    Should Be True    ${response.status_code} == 202 or ${response.status_code} == 502

Valid Event Type PAYMENT_FAILED Passes Validation
    [Documentation]    PAYMENT_FAILED passes validation
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=PAYMENT_FAILED
    ...    userId=456
    ...    email=payment@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=any
    Should Be True    ${response.status_code} == 202 or ${response.status_code} == 502

Valid Event Type ORDER_SHIPPED Passes Validation
    [Documentation]    ORDER_SHIPPED passes validation
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=ORDER_SHIPPED
    ...    userId=789
    ...    email=order@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=any
    Should Be True    ${response.status_code} == 202 or ${response.status_code} == 502

Valid Event Type SECURITY_ALERT Passes Validation
    [Documentation]    SECURITY_ALERT passes validation
    [Tags]    validation    events
    ${payload}=    Create Dictionary
    ...    eventType=SECURITY_ALERT
    ...    userId=admin
    ...    email=security@email.com
    ${response}=    POST On Session    gateway    /events    json=${payload}    expected_status=any
    Should Be True    ${response.status_code} == 202 or ${response.status_code} == 502
