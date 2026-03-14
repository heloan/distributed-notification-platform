"""
Gateway — Observability Integration Tests.
Validates Prometheus /metrics endpoint and Swagger docs availability.
"""

import pytest


@pytest.mark.observability
@pytest.mark.gateway
class TestObservability:
    """Verify the API Gateway exposes observability endpoints."""

    # ----- Prometheus metrics ------------------------------------------------

    def test_metrics_endpoint_returns_200(self, api_session, gateway_url):
        """GET /metrics should return 200 OK."""
        response = api_session.get(f"{gateway_url}/metrics")
        assert response.status_code == 200

    def test_metrics_contains_http_requests(self, api_session, gateway_url):
        """Prometheus output should contain HTTP request metrics."""
        response = api_session.get(f"{gateway_url}/metrics")
        body = response.text
        assert "http" in body.lower(), "Expected HTTP-related metrics"

    # ----- Swagger / OpenAPI ------------------------------------------------

    def test_swagger_index_returns_200(self, api_session, gateway_url):
        """GET /swagger/index.html should return 200 OK."""
        response = api_session.get(f"{gateway_url}/swagger/index.html")
        assert response.status_code == 200

    def test_swagger_json_returns_200(self, api_session, gateway_url):
        """GET /swagger/v1/swagger.json should return valid OpenAPI spec."""
        response = api_session.get(f"{gateway_url}/swagger/v1/swagger.json")
        assert response.status_code == 200
        body = response.json()
        assert "openapi" in body or "swagger" in body
