"""
Gateway — Health Endpoint Integration Tests.
Validates /health, /health/ready, /health/live endpoints.
"""

import pytest


@pytest.mark.smoke
@pytest.mark.health
@pytest.mark.gateway
class TestHealthEndpoints:
    """Verify the API Gateway health check endpoints."""

    def test_health_returns_200(self, api_session, gateway_url):
        """GET /health should return 200 OK."""
        response = api_session.get(f"{gateway_url}/health")
        assert response.status_code == 200

    def test_health_response_body(self, api_session, gateway_url):
        """GET /health response should contain 'status' key."""
        response = api_session.get(f"{gateway_url}/health")
        body = response.json()
        assert "status" in body
        assert body["status"].lower() == "healthy"

    def test_health_ready_returns_200(self, api_session, gateway_url):
        """GET /health/ready should return 200 OK when all dependencies are up."""
        response = api_session.get(f"{gateway_url}/health/ready")
        assert response.status_code == 200

    def test_health_live_returns_200(self, api_session, gateway_url):
        """GET /health/live should always return 200 OK."""
        response = api_session.get(f"{gateway_url}/health/live")
        assert response.status_code == 200
