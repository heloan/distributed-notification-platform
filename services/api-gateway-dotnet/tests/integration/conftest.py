"""
Shared fixtures and configuration for API Gateway integration tests.
"""

import os
import pytest
import requests

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------

BASE_URL = os.getenv("GATEWAY_BASE_URL", "http://localhost:5000")


# ---------------------------------------------------------------------------
# Fixtures
# ---------------------------------------------------------------------------

@pytest.fixture(scope="session")
def base_url():
    """Base URL of the running API Gateway."""
    return BASE_URL


@pytest.fixture(scope="session")
def api_session():
    """Reusable HTTP session with default headers."""
    session = requests.Session()
    session.headers.update({
        "Content-Type": "application/json",
        "Accept": "application/json",
    })
    return session


@pytest.fixture
def valid_event_payload():
    """A valid USER_REGISTERED event payload."""
    return {
        "eventType": "USER_REGISTERED",
        "userId": "test-user-123",
        "email": "testuser@email.com",
        "timestamp": "2026-03-14T10:00:00"
    }


@pytest.fixture
def valid_payment_failed_payload():
    """A valid PAYMENT_FAILED event payload."""
    return {
        "eventType": "PAYMENT_FAILED",
        "userId": "test-user-456",
        "email": "payment@email.com"
    }


@pytest.fixture
def invalid_event_type_payload():
    """An event payload with an unsupported event type."""
    return {
        "eventType": "INVALID_TYPE",
        "userId": "123",
        "email": "user@email.com"
    }


@pytest.fixture
def empty_fields_payload():
    """An event payload with all required fields empty."""
    return {
        "eventType": "",
        "userId": "",
        "email": ""
    }
