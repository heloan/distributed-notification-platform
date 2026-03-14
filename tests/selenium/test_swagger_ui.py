"""
Selenium — Swagger UI Browser Tests.
Validates the API Gateway's Swagger documentation renders correctly.
"""

import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC


class TestSwaggerUI:
    """Verify the Swagger UI page renders and is interactive."""

    def test_swagger_page_loads(self, browser, gateway_url):
        """Swagger UI should load without errors."""
        browser.get(f"{gateway_url}/swagger/index.html")
        assert "Swagger" in browser.title or "swagger" in browser.page_source.lower()

    def test_swagger_has_api_title(self, browser, gateway_url):
        """The Swagger page should show the API title."""
        browser.get(f"{gateway_url}/swagger/index.html")
        wait = WebDriverWait(browser, 10)
        title = wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, ".title")))
        assert title.text != ""

    def test_swagger_shows_endpoints(self, browser, gateway_url):
        """Swagger UI should list at least one API endpoint."""
        browser.get(f"{gateway_url}/swagger/index.html")
        wait = WebDriverWait(browser, 10)
        operations = wait.until(
            EC.presence_of_all_elements_located((By.CSS_SELECTOR, ".opblock"))
        )
        assert len(operations) > 0, "Expected at least one API operation block"

    def test_swagger_post_events_endpoint_visible(self, browser, gateway_url):
        """POST /api/events should be visible in Swagger UI."""
        browser.get(f"{gateway_url}/swagger/index.html")
        wait = WebDriverWait(browser, 10)
        wait.until(EC.presence_of_element_located((By.CSS_SELECTOR, ".opblock")))
        page_text = browser.find_element(By.TAG_NAME, "body").text
        assert "/api/events" in page_text

    def test_swagger_try_it_out_button_exists(self, browser, gateway_url):
        """Each endpoint should have a 'Try it out' button."""
        browser.get(f"{gateway_url}/swagger/index.html")
        wait = WebDriverWait(browser, 10)

        # Expand the first operation
        first_op = wait.until(EC.element_to_be_clickable((By.CSS_SELECTOR, ".opblock")))
        first_op.click()

        try_btn = wait.until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".try-out__btn"))
        )
        assert try_btn.is_displayed()

    def test_swagger_json_accessible(self, browser, gateway_url):
        """The raw OpenAPI JSON spec should be accessible."""
        browser.get(f"{gateway_url}/swagger/v1/swagger.json")
        body_text = browser.find_element(By.TAG_NAME, "body").text
        assert "openapi" in body_text.lower() or "swagger" in body_text.lower()
