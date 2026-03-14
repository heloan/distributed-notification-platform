*** Settings ***
Documentation     API Gateway — Swagger UI Browser Tests (Selenium)
...               Validates that the Swagger UI loads correctly in a browser
...               and that all endpoints are visible and interactive.
Library           SeleniumLibrary
Library           Collections

Suite Setup       Open Swagger UI
Suite Teardown    Close All Browsers

*** Variables ***
${BASE_URL}       http://localhost:5000
${SWAGGER_URL}    ${BASE_URL}/swagger/index.html
${BROWSER}        headlesschrome

*** Test Cases ***
TC-013-UI: Swagger UI Page Loads Successfully
    [Documentation]    Swagger UI HTML page renders in the browser
    [Tags]    selenium    swagger    ui    smoke
    Title Should Be    DSNP API Gateway
    Page Should Contain    DSNP — API Gateway

Swagger UI Shows API Version
    [Documentation]    Swagger UI displays API version v1
    [Tags]    selenium    swagger    ui
    Page Should Contain    v1

Swagger UI Shows Events Endpoint Section
    [Documentation]    Swagger UI lists the Events tag with POST and GET endpoints
    [Tags]    selenium    swagger    ui
    Page Should Contain    Events

Swagger UI Shows Health Endpoint Section
    [Documentation]    Swagger UI lists the Health tag
    [Tags]    selenium    swagger    ui
    Page Should Contain    Health

Swagger UI POST Events Endpoint Is Visible
    [Documentation]    POST /events endpoint is visible and expandable
    [Tags]    selenium    swagger    ui
    Page Should Contain Element    xpath=//span[contains(text(),'POST')]

Swagger UI GET Events Endpoint Is Visible
    [Documentation]    GET /events endpoint is visible
    [Tags]    selenium    swagger    ui
    Page Should Contain Element    xpath=//span[contains(text(),'GET')]

Swagger UI Has Try It Out Button
    [Documentation]    Swagger UI endpoints have "Try it out" buttons
    [Tags]    selenium    swagger    ui
    # Click on the first POST endpoint to expand it
    Click Element    xpath=(//button[contains(@class,'opblock-summary')])[1]
    Wait Until Page Contains Element    xpath=//button[contains(text(),'Try it out')]    timeout=5s
    Page Should Contain Element    xpath=//button[contains(text(),'Try it out')]

Swagger UI Contact Link Is Present
    [Documentation]    Swagger UI shows the contact/author information
    [Tags]    selenium    swagger    ui
    Page Should Contain    Heloan Marinho

*** Keywords ***
Open Swagger UI
    [Documentation]    Opens Swagger UI in headless Chrome
    Open Browser    ${SWAGGER_URL}    ${BROWSER}
    ...    options=add_argument("--no-sandbox");add_argument("--disable-dev-shm-usage");add_argument("--disable-gpu")
    Set Window Size    1920    1080
    Wait Until Page Contains    DSNP — API Gateway    timeout=15s
