*** Settings ***
Documentation    API Gateway — Health endpoint tests.
Resource         ../resources/common.resource
Suite Setup      Create Gateway Session

*** Test Cases ***
Health Endpoint Returns 200
    [Documentation]    GET /health should return HTTP 200.
    [Tags]             smoke    health    gateway
    ${response}=       GET On Session    gateway    /health
    Response Status Should Be    ${response}    200

Health Response Contains Status Key
    [Documentation]    Health response body should contain 'status' key.
    [Tags]             health    gateway
    ${response}=       GET On Session    gateway    /health
    Response Should Contain Key    ${response}    status

Health Ready Returns 200
    [Documentation]    GET /health/ready should return 200 when dependencies are up.
    [Tags]             health    gateway
    ${response}=       GET On Session    gateway    /health/ready
    Response Status Should Be    ${response}    200

Health Live Returns 200
    [Documentation]    GET /health/live should always return 200.
    [Tags]             smoke    health    gateway
    ${response}=       GET On Session    gateway    /health/live
    Response Status Should Be    ${response}    200
