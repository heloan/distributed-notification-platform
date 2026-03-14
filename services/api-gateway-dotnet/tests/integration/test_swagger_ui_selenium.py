"""
Selenium — Swagger UI Integration Tests

Browser-based tests using Python + Selenium to validate
that the Swagger UI renders correctly and is interactive.
Requires: API Gateway running on localhost:5000, Chrome/Chromium installed.
"""

import pytest
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager


SWAGGER_URL = "http://localhost:5000/swagger/index.html"


@pytest.fixture(scope="module")
def driver():
    """Headless Chrome WebDriver."""
    chrome_options = Options()
    chrome_options.add_argument("--headless")
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--disable-dev-shm-usage")
    chrome_options.add_argument("--disable-gpu")
    chrome_options.add_argument("--window-size=1920,1080")

    service = Service(ChromeDriverManager().install())
    browser = webdriver.Chrome(service=service, options=chrome_options)
    browser.implicitly_wait(10)

    yield browser

    browser.quit()


class TestSwaggerUI:
    """Selenium tests for Swagger UI."""

    def test_swagger_page_loads(self, driver):
        """TC-013-UI: Swagger UI page loads and shows the API title."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.CLASS_NAME, "title"))
        )

        assert "DSNP" in driver.title or "API Gateway" in driver.title

    def test_swagger_shows_api_title(self, driver):
        """Swagger UI displays 'DSNP — API Gateway' title."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.CLASS_NAME, "title"))
        )

        title_element = driver.find_element(By.CLASS_NAME, "title")
        assert "DSNP" in title_element.text

    def test_swagger_shows_events_tag(self, driver):
        """Swagger UI shows the 'Events' endpoint group."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.TAG_NAME, "body"))
        )

        body_text = driver.find_element(By.TAG_NAME, "body").text
        assert "Events" in body_text

    def test_swagger_shows_health_tag(self, driver):
        """Swagger UI shows the 'Health' endpoint group."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.TAG_NAME, "body"))
        )

        body_text = driver.find_element(By.TAG_NAME, "body").text
        assert "Health" in body_text

    def test_swagger_has_post_endpoint(self, driver):
        """Swagger UI shows at least one POST endpoint."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.CLASS_NAME, "opblock-post"))
        )

        post_blocks = driver.find_elements(By.CLASS_NAME, "opblock-post")
        assert len(post_blocks) >= 1

    def test_swagger_has_get_endpoints(self, driver):
        """Swagger UI shows GET endpoints."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.CLASS_NAME, "opblock-get"))
        )

        get_blocks = driver.find_elements(By.CLASS_NAME, "opblock-get")
        assert len(get_blocks) >= 2  # /events, /events/{id}, /health, etc.

    def test_swagger_endpoint_expandable(self, driver):
        """Clicking an endpoint expands its details."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.element_to_be_clickable((By.CSS_SELECTOR, ".opblock-summary"))
        )

        summary = driver.find_element(By.CSS_SELECTOR, ".opblock-summary")
        summary.click()

        WebDriverWait(driver, 5).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, ".try-out__btn"))
        )

        try_btn = driver.find_element(By.CSS_SELECTOR, ".try-out__btn")
        assert try_btn.is_displayed()

    def test_swagger_shows_contact_info(self, driver):
        """Swagger UI displays the developer contact information."""
        driver.get(SWAGGER_URL)

        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.TAG_NAME, "body"))
        )

        body_text = driver.find_element(By.TAG_NAME, "body").text
        assert "Heloan Marinho" in body_text
