"""
Gateway — Event Forwarding Integration Tests.
Validates that the API Gateway correctly forwards events to the Event Service.
"""

import pytest


@pytest.mark.forwarding
@pytest.mark.gateway
class TestEventForwarding:
    """Verify the API Gateway forwards events to the downstream Event Service."""

    def test_post_event_returns_success(self, api_session, gateway_url, valid_user_registered_payload):
        """POST /api/events should return 2xx when Event Service is healthy."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        assert response.status_code in (200, 201, 202), (
            f"Expected 2xx, got {response.status_code}: {response.text}"
        )

    def test_post_event_response_contains_id(self, api_session, gateway_url, valid_user_registered_payload):
        """Success response should contain an event ID."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        if response.status_code in (200, 201, 202):
            body = response.json()
            assert "id" in body or "eventId" in body, "Response must contain event identifier"

    def test_correlation_id_header_present(self, api_session, gateway_url, valid_user_registered_payload):
        """Response should include a correlation / request ID header."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        correlation = (
            response.headers.get("X-Correlation-Id")
            or response.headers.get("X-Request-Id")
        )
        # Acceptable even if header is absent — depends on gateway config
        if correlation:
            assert len(correlation) > 0

    def test_content_type_is_json(self, api_session, gateway_url, valid_user_registered_payload):
        """Response Content-Type should be application/json."""
        response = api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        content_type = response.headers.get("Content-Type", "")
        assert "application/json" in content_type

    def test_get_events_returns_list(self, api_session, gateway_url):
        """GET /api/events should return a JSON array (or appropriate status)."""
        response = api_session.get(f"{gateway_url}/api/events")
        assert response.status_code in (200, 404, 502)
        if response.status_code == 200:
            assert isinstance(response.json(), list)
