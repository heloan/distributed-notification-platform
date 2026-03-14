"""
Integration Tests — Event Endpoints (Forwarding)

Tests the /events endpoints focusing on downstream communication.
These tests check behavior when Event Service is available or unavailable.
Requires: API Gateway running on localhost:5000
"""

import pytest


class TestEventForwarding:
    """Tests for event forwarding to downstream Event Service."""

    def test_submit_event_returns_202_or_502(self, api_session, base_url, valid_event_payload):
        """TC-003: Valid event returns 202 (service up) or 502 (service down)."""
        response = api_session.post(f"{base_url}/events", json=valid_event_payload)

        # 202 = forwarded successfully, 502 = downstream unavailable
        assert response.status_code in [202, 502]

        if response.status_code == 202:
            body = response.json()
            assert "id" in body
            assert body["eventType"] == "USER_REGISTERED"

        if response.status_code == 502:
            body = response.json()
            assert body["title"] == "Bad Gateway"
            assert body["status"] == 502

    def test_get_events_returns_200_or_502(self, api_session, base_url):
        """TC-011: GET /events returns 200 (service up) or 502 (service down)."""
        response = api_session.get(f"{base_url}/events")

        assert response.status_code in [200, 502]

    def test_get_event_by_id_returns_response(self, api_session, base_url):
        """TC-012: GET /events/{id} returns 404, 200, or 502."""
        response = api_session.get(f"{base_url}/events/non-existent-id")

        assert response.status_code in [404, 502]

    def test_bad_gateway_response_format(self, api_session, base_url, valid_event_payload):
        """502 response follows the standard ErrorResponse format."""
        response = api_session.post(f"{base_url}/events", json=valid_event_payload)

        if response.status_code == 502:
            body = response.json()
            assert "title" in body
            assert "status" in body
            assert "detail" in body
            assert body["status"] == 502


class TestEventPayloadFormats:
    """Tests for various payload edge cases."""

    def test_null_timestamp_accepted(self, api_session, base_url):
        """Null timestamp is valid and accepted."""
        payload = {
            "eventType": "USER_REGISTERED",
            "userId": "123",
            "email": "user@email.com"
        }

        response = api_session.post(f"{base_url}/events", json=payload)

        # Should not be 400 (validation passes)
        assert response.status_code in [202, 502]

    def test_empty_body_returns_error(self, api_session, base_url):
        """Empty request body returns an error."""
        response = api_session.post(
            f"{base_url}/events",
            data="",
            headers={"Content-Type": "application/json"}
        )

        assert response.status_code in [400, 415]
