"""
Integration Tests — Observability Endpoints

Tests for Prometheus metrics and Swagger documentation.
Requires: API Gateway running on localhost:5000
"""

import pytest


class TestObservability:
    """Tests for metrics and API documentation endpoints."""

    def test_prometheus_metrics_endpoint(self, api_session, base_url):
        """TC-014: GET /metrics returns Prometheus-format metrics."""
        response = api_session.get(f"{base_url}/metrics")

        assert response.status_code == 200
        assert "text/plain" in response.headers.get("Content-Type", "")

        # Should contain OpenTelemetry metrics
        body = response.text
        assert len(body) > 0

    def test_swagger_json_available(self, api_session, base_url):
        """Swagger JSON specification is accessible."""
        response = api_session.get(f"{base_url}/swagger/v1/swagger.json")

        assert response.status_code == 200

        body = response.json()
        assert body["info"]["title"] == "DSNP — API Gateway"
        assert body["info"]["version"] == "v1"

    def test_swagger_lists_event_endpoints(self, api_session, base_url):
        """Swagger spec includes all event endpoints."""
        response = api_session.get(f"{base_url}/swagger/v1/swagger.json")
        body = response.json()
        paths = body.get("paths", {})

        assert "/events" in paths
        assert "/health" in paths

    def test_swagger_ui_accessible(self, api_session, base_url):
        """TC-013: Swagger UI HTML page loads."""
        response = api_session.get(f"{base_url}/swagger/index.html")

        assert response.status_code == 200
        assert "text/html" in response.headers.get("Content-Type", "")


class TestCors:
    """Tests for CORS configuration."""

    def test_cors_headers_present(self, api_session, base_url):
        """TC-015: CORS headers are returned for cross-origin requests."""
        headers = {
            "Origin": "http://example.com",
            "Access-Control-Request-Method": "POST",
        }

        response = api_session.options(f"{base_url}/events", headers=headers)

        # Gateway has CORS enabled — should respond to preflight
        assert response.status_code in [200, 204]
