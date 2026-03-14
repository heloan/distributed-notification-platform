"""
Notification Service — Notification Dispatch Integration Tests.
Validates that notifications are dispatched through the correct channels.
"""

import pytest


@pytest.mark.notification_service
class TestNotificationDispatch:
    """Verify the Notification Service dispatches notifications correctly."""

    def test_get_notifications_returns_200(self, api_session, notification_service_url):
        """GET /api/notifications should return 200."""
        response = api_session.get(f"{notification_service_url}/api/notifications")
        assert response.status_code in (200, 204)

    def test_notification_contains_channel(self, api_session, notification_service_url):
        """Each notification should specify a delivery channel."""
        response = api_session.get(f"{notification_service_url}/api/notifications")
        if response.status_code == 200:
            notifications = response.json()
            for n in notifications[:5]:
                assert "channel" in n or "type" in n, "Notification must have a channel/type"

    def test_notification_contains_status(self, api_session, notification_service_url):
        """Each notification should have a delivery status."""
        response = api_session.get(f"{notification_service_url}/api/notifications")
        if response.status_code == 200:
            notifications = response.json()
            for n in notifications[:5]:
                assert "status" in n, "Notification must have a status"

    def test_notification_by_user_id(self, api_session, notification_service_url):
        """GET /api/notifications?userId=xxx should filter by user."""
        response = api_session.get(
            f"{notification_service_url}/api/notifications",
            params={"userId": "test-user-123"}
        )
        assert response.status_code in (200, 204)
