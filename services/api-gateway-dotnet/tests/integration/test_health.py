"""
Integration Tests — Health Endpoints

Tests the /health and /health/ready endpoints of the API Gateway.
Requires: API Gateway running on localhost:5000
"""

import pytest


class TestHealthEndpoints:
    """Tests for health check and readiness endpoints."""

    def test_health_returns_200(self, api_session, base_url):
        """TC-001: GET /health returns 200 with healthy status."""
        response = api_session.get(f"{base_url}/health")

        assert response.status_code == 200

        body = response.json()
        assert body["status"] == "Healthy"
        assert body["service"] == "API Gateway"
        assert "timestamp" in body

    def test_health_response_has_correct_content_type(self, api_session, base_url):
        """Health endpoint returns JSON content type."""
        response = api_session.get(f"{base_url}/health")

        assert "application/json" in response.headers.get("Content-Type", "")

    def test_readiness_returns_response(self, api_session, base_url):
        """TC-002: GET /health/ready returns a valid response (200 or 503)."""
        response = api_session.get(f"{base_url}/health/ready")

        # Either healthy (200) or unhealthy (503) is valid depending on Event Service state
        assert response.status_code in [200, 503]

        body = response.json()
        assert body["status"] in ["Ready", "Degraded", "Unhealthy"]
        assert body["service"] == "API Gateway"
