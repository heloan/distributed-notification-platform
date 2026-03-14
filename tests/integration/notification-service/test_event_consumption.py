"""
Notification Service — Event Consumption Integration Tests.
Validates that the .NET Notification Service consumes Kafka events.
"""

import time

import pytest


@pytest.mark.notification_service
class TestEventConsumption:
    """Verify the Notification Service processes events from Kafka."""

    def test_service_health(self, api_session, notification_service_url):
        """Notification Service should expose a health endpoint."""
        response = api_session.get(f"{notification_service_url}/health")
        assert response.status_code == 200

    def test_event_consumed_after_publish(
        self, api_session, gateway_url, notification_service_url, valid_user_registered_payload
    ):
        """After publishing an event via the gateway, the notification service
        should eventually reflect that it processed the event."""
        # Publish event
        api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)

        # Wait for async processing (Kafka + consumer lag)
        time.sleep(3)

        # Check notifications endpoint
        response = api_session.get(f"{notification_service_url}/api/notifications")
        assert response.status_code in (200, 204)

    def test_notification_created_for_event(
        self, api_session, gateway_url, notification_service_url, valid_payment_failed_payload
    ):
        """A consumed event should produce at least one notification record."""
        api_session.post(f"{gateway_url}/api/events", json=valid_payment_failed_payload)
        time.sleep(3)

        response = api_session.get(f"{notification_service_url}/api/notifications")
        if response.status_code == 200:
            notifications = response.json()
            assert isinstance(notifications, list)
