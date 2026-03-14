"""
Event Service — Event Creation Integration Tests.
Validates that the Java Event Service correctly creates and persists events.
"""

import pytest


@pytest.mark.event_service
class TestEventCreation:
    """Verify the Event Service creates events via its REST API."""

    def test_create_event_returns_201(self, api_session, event_service_url, valid_user_registered_payload):
        """POST /api/events should return 201 Created."""
        response = api_session.post(f"{event_service_url}/api/events", json=valid_user_registered_payload)
        assert response.status_code == 201, f"Expected 201, got {response.status_code}: {response.text}"

    def test_created_event_has_id(self, api_session, event_service_url, valid_user_registered_payload):
        """Created event response should contain an ID."""
        response = api_session.post(f"{event_service_url}/api/events", json=valid_user_registered_payload)
        if response.status_code == 201:
            body = response.json()
            assert "id" in body, "Created event must have an 'id' field"

    def test_create_event_all_types(self, api_session, event_service_url):
        """All supported event types should be creatable."""
        types = ["USER_REGISTERED", "PAYMENT_FAILED", "ORDER_SHIPPED", "SECURITY_ALERT"]
        for event_type in types:
            payload = {"eventType": event_type, "userId": "u-1", "email": "u@e.com"}
            response = api_session.post(f"{event_service_url}/api/events", json=payload)
            assert response.status_code in (200, 201, 202), (
                f"Event type {event_type} failed: {response.status_code}"
            )

    def test_create_event_invalid_type_returns_400(self, api_session, event_service_url):
        """Unsupported event type should be rejected."""
        payload = {"eventType": "UNSUPPORTED", "userId": "u-1", "email": "u@e.com"}
        response = api_session.post(f"{event_service_url}/api/events", json=payload)
        assert response.status_code == 400
