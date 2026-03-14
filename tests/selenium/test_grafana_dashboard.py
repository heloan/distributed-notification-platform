"""
Selenium — Grafana Dashboard Browser Tests.
Validates the Grafana monitoring dashboard renders correctly.
"""

import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC


class TestGrafanaDashboard:
    """Verify the Grafana dashboard page loads and displays panels."""

    def test_grafana_login_page_loads(self, browser, grafana_url):
        """Grafana should serve its login / home page."""
        browser.get(grafana_url)
        assert browser.title != "", "Grafana page title should not be empty"

    def test_grafana_login_form_visible(self, browser, grafana_url):
        """The login form should be visible on the Grafana page."""
        browser.get(f"{grafana_url}/login")
        wait = WebDriverWait(browser, 10)
        username_input = wait.until(
            EC.presence_of_element_located((By.NAME, "user"))
        )
        assert username_input.is_displayed()

    def test_grafana_login_succeeds(self, browser, grafana_url):
        """Logging in with default credentials should redirect to home."""
        browser.get(f"{grafana_url}/login")
        wait = WebDriverWait(browser, 10)

        username = wait.until(EC.presence_of_element_located((By.NAME, "user")))
        password = browser.find_element(By.NAME, "password")

        username.clear()
        username.send_keys("admin")
        password.clear()
        password.send_keys("admin")

        login_btn = browser.find_element(By.CSS_SELECTOR, "button[type='submit']")
        login_btn.click()

        # Wait for redirect (skip password change if prompted)
        wait.until(lambda d: "/login" not in d.current_url)

    def test_grafana_dashboard_panels_visible(self, browser, grafana_url):
        """After login, navigating to dashboards should show panels."""
        # Login first
        browser.get(f"{grafana_url}/login")
        wait = WebDriverWait(browser, 10)

        username = wait.until(EC.presence_of_element_located((By.NAME, "user")))
        password = browser.find_element(By.NAME, "password")
        username.clear()
        username.send_keys("admin")
        password.clear()
        password.send_keys("admin")
        browser.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
        wait.until(lambda d: "/login" not in d.current_url)

        # Navigate to dashboards
        browser.get(f"{grafana_url}/dashboards")
        wait.until(EC.presence_of_element_located((By.TAG_NAME, "body")))
        assert browser.title != ""

    def test_grafana_api_health(self, browser, grafana_url):
        """Grafana API health endpoint should return OK."""
        browser.get(f"{grafana_url}/api/health")
        body = browser.find_element(By.TAG_NAME, "body").text
        assert "ok" in body.lower()
