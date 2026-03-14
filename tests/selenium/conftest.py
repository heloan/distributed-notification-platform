"""
Selenium test fixtures — shared WebDriver management.
"""

import pytest
from selenium import webdriver
from selenium.webdriver.chrome.options import Options as ChromeOptions
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager


GATEWAY_URL = "http://localhost:5000"
GRAFANA_URL = "http://localhost:3000"


@pytest.fixture(scope="session")
def gateway_url():
    return GATEWAY_URL


@pytest.fixture(scope="session")
def grafana_url():
    return GRAFANA_URL


@pytest.fixture(scope="function")
def browser():
    """Create a headless Chrome browser instance for each test."""
    options = ChromeOptions()
    options.add_argument("--headless=new")
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    options.add_argument("--disable-gpu")
    options.add_argument("--window-size=1920,1080")

    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=options)
    driver.implicitly_wait(10)

    yield driver

    driver.quit()
