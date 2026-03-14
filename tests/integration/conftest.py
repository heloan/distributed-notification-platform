"""
Shared fixtures and configuration for all integration tests.
Centralised configuration for all service URLs and reusable HTTP sessions.
"""

import os
import pytest
import requests

# ---------------------------------------------------------------------------
# Service URLs (configurable via environment variables)
# ---------------------------------------------------------------------------

GATEWAY_URL = os.getenv("GATEWAY_BASE_URL", "http://localhost:5000")
EVENT_SERVICE_URL = os.getenv("EVENT_SERVICE_BASE_URL", "http://localhost:8080")
NOTIFICATION_SERVICE_URL = os.getenv("NOTIFICATION_SERVICE_BASE_URL", "http://localhost:5001")


# ---------------------------------------------------------------------------
# Session Fixtures
# ---------------------------------------------------------------------------

@pytest.fixture(scope="session")
def gateway_url():
    """Base URL of the API Gateway."""
    return GATEWAY_URL


@pytest.fixture(scope="session")
def event_service_url():
    """Base URL of the Event Service (Java)."""
    return EVENT_SERVICE_URL


@pytest.fixture(scope="session")
def notification_service_url():
    """Base URL of the Notification Service (.NET)."""
    return NOTIFICATION_SERVICE_URL


@pytest.fixture(scope="session")
def api_session():
    """Reusable HTTP session with default headers."""
    session = requests.Session()
    session.headers.update({
        "Content-Type": "application/json",
        "Accept": "application/json",
    })
    return session


# ---------------------------------------------------------------------------
# Event Payload Fixtures
# ---------------------------------------------------------------------------

@pytest.fixture
def valid_user_registered_payload():
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
def valid_order_shipped_payload():
    """A valid ORDER_SHIPPED event payload."""
    return {
        "eventType": "ORDER_SHIPPED",
        "userId": "test-user-789",
        "email": "order@email.com"
    }


@pytest.fixture
def valid_security_alert_payload():
    """A valid SECURITY_ALERT event payload."""
    return {
        "eventType": "SECURITY_ALERT",
        "userId": "admin-001",
        "email": "security@email.com"
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
