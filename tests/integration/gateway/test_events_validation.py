"""
Gateway — Event Validation Integration Tests.
Validates FluentValidation rules enforced by the API Gateway.
"""

import pytest


@pytest.mark.validation
@pytest.mark.gateway
class TestEventValidation:
    """Validate request-level validation rules on POST /api/events."""

    # ----- Happy path -------------------------------------------------------

    def test_valid_user_registered(self, api_session, gateway_url, valid_user_registered_payload):
        """A well-formed USER_REGISTERED event should be accepted (not 400)."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        assert response.status_code != 400, f"Valid payload rejected: {response.text}"

    def test_valid_payment_failed(self, api_session, gateway_url, valid_payment_failed_payload):
        """A well-formed PAYMENT_FAILED event should be accepted."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_payment_failed_payload)
        assert response.status_code != 400

    def test_valid_order_shipped(self, api_session, gateway_url, valid_order_shipped_payload):
        """A well-formed ORDER_SHIPPED event should be accepted."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_order_shipped_payload)
        assert response.status_code != 400

    def test_valid_security_alert(self, api_session, gateway_url, valid_security_alert_payload):
        """A well-formed SECURITY_ALERT event should be accepted."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_security_alert_payload)
        assert response.status_code != 400

    # ----- Negative / edge-case paths ---------------------------------------

    def test_invalid_event_type_returns_400(self, api_session, gateway_url, invalid_event_type_payload):
        """Unsupported event type must return 400."""
        response = api_session.post(f"{gateway_url}/api/events", json=invalid_event_type_payload)
        assert response.status_code == 400

    def test_invalid_event_type_error_mentions_event_type(self, api_session, gateway_url, invalid_event_type_payload):
        """Error body should mention the unsupported event type."""
        response = api_session.post(f"{gateway_url}/api/events", json=invalid_event_type_payload)
        body = response.json()
        errors = str(body.get("errors", body))
        assert "EventType" in errors or "eventType" in errors or "event type" in errors.lower()

    def test_empty_fields_return_400(self, api_session, gateway_url, empty_fields_payload):
        """Empty required fields must return 400."""
        response = api_session.post(f"{gateway_url}/api/events", json=empty_fields_payload)
        assert response.status_code == 400

    def test_empty_body_returns_400(self, api_session, gateway_url):
        """An empty JSON body should return 400."""
        response = api_session.post(f"{gateway_url}/api/events", json={})
        assert response.status_code == 400

    def test_missing_user_id_returns_400(self, api_session, gateway_url):
        """Missing userId should return 400."""
        payload = {"eventType": "USER_REGISTERED", "email": "user@email.com"}
        response = api_session.post(f"{gateway_url}/api/events", json=payload)
        assert response.status_code == 400

    def test_missing_email_returns_400(self, api_session, gateway_url):
        """Missing email should return 400."""
        payload = {"eventType": "USER_REGISTERED", "userId": "123"}
        response = api_session.post(f"{gateway_url}/api/events", json=payload)
        assert response.status_code == 400

    def test_invalid_email_format_returns_400(self, api_session, gateway_url):
        """Invalid email format should return 400."""
        payload = {"eventType": "USER_REGISTERED", "userId": "123", "email": "not-an-email"}
        response = api_session.post(f"{gateway_url}/api/events", json=payload)
        assert response.status_code == 400
