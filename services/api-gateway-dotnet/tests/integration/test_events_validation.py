"""
Integration Tests — Event Endpoints (Validation)

Tests the /events endpoints focusing on input validation.
These tests do NOT require the Event Service to be running.
Requires: API Gateway running on localhost:5000
"""

import pytest


class TestEventValidation:
    """Tests for event request validation (400 responses)."""

    def test_invalid_event_type_returns_400(self, api_session, base_url, invalid_event_type_payload):
        """TC-005: Unsupported event type returns 400 with validation error."""
        response = api_session.post(f"{base_url}/events", json=invalid_event_type_payload)

        assert response.status_code == 400

        body = response.json()
        assert body["title"] == "Validation Failed"
        assert body["status"] == 400
        assert "EventType" in body["errors"]

    def test_empty_email_returns_400(self, api_session, base_url):
        """TC-006: Empty email returns 400 with validation error."""
        payload = {
            "eventType": "USER_REGISTERED",
            "userId": "123",
            "email": ""
        }

        response = api_session.post(f"{base_url}/events", json=payload)

        assert response.status_code == 400

        body = response.json()
        assert "Email" in body["errors"]

    def test_invalid_email_format_returns_400(self, api_session, base_url):
        """TC-007: Invalid email format returns 400."""
        payload = {
            "eventType": "USER_REGISTERED",
            "userId": "123",
            "email": "not-an-email"
        }

        response = api_session.post(f"{base_url}/events", json=payload)

        assert response.status_code == 400

        body = response.json()
        assert "Email" in body["errors"]

    def test_empty_user_id_returns_400(self, api_session, base_url):
        """TC-008: Empty userId returns 400."""
        payload = {
            "eventType": "USER_REGISTERED",
            "userId": "",
            "email": "user@email.com"
        }

        response = api_session.post(f"{base_url}/events", json=payload)

        assert response.status_code == 400

        body = response.json()
        assert "UserId" in body["errors"]

    def test_future_timestamp_returns_400(self, api_session, base_url):
        """TC-009: Future timestamp returns 400."""
        payload = {
            "eventType": "USER_REGISTERED",
            "userId": "123",
            "email": "user@email.com",
            "timestamp": "2030-01-01T00:00:00"
        }

        response = api_session.post(f"{base_url}/events", json=payload)

        assert response.status_code == 400

        body = response.json()
        assert "Timestamp" in body["errors"]

    def test_multiple_validation_errors_returned(self, api_session, base_url, empty_fields_payload):
        """TC-010: All validation errors returned at once."""
        response = api_session.post(f"{base_url}/events", json=empty_fields_payload)

        assert response.status_code == 400

        body = response.json()
        assert body["title"] == "Validation Failed"
        assert len(body["errors"]) >= 3

    @pytest.mark.parametrize("event_type", [
        "USER_REGISTERED",
        "PAYMENT_FAILED",
        "ORDER_SHIPPED",
        "SECURITY_ALERT",
    ])
    def test_valid_event_types_accepted(self, api_session, base_url, event_type):
        """Valid event types pass validation (may get 202 or 502 depending on Event Service)."""
        payload = {
            "eventType": event_type,
            "userId": "123",
            "email": "user@email.com"
        }

        response = api_session.post(f"{base_url}/events", json=payload)

        # 202 = Event Service running, 502 = Event Service down (both pass validation)
        assert response.status_code in [202, 502]
