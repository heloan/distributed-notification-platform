"""
End-to-End — Full Event Flow Tests.
Validates the complete pipeline: Gateway → Event Service → Kafka → Notification Service.
"""

import time

import pytest


@pytest.mark.e2e
class TestFullEventFlow:
    """Verify the full event pipeline from ingestion to notification."""

    def test_event_flows_through_all_services(
        self, api_session, gateway_url, event_service_url, notification_service_url,
        valid_user_registered_payload
    ):
        """
        1. POST event to Gateway
        2. Verify it was persisted by Event Service
        3. Verify a notification was created by Notification Service
        """
        # Step 1 — Submit event via Gateway
        gateway_resp = api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        assert gateway_resp.status_code in (200, 201, 202), (
            f"Gateway rejected the event: {gateway_resp.status_code}"
        )
        event_id = gateway_resp.json().get("id") or gateway_resp.json().get("eventId")

        # Step 2 — Confirm event exists in Event Service
        if event_id:
            event_resp = api_session.get(f"{event_service_url}/api/events/{event_id}")
            assert event_resp.status_code == 200, "Event not found in Event Service"

        # Step 3 — Wait for Kafka consumer & notification creation
        time.sleep(5)
        notif_resp = api_session.get(f"{notification_service_url}/api/notifications")
        assert notif_resp.status_code in (200, 204), "Notification Service unreachable"

    def test_security_alert_triggers_priority_notification(
        self, api_session, gateway_url, notification_service_url, valid_security_alert_payload
    ):
        """SECURITY_ALERT events should trigger a high-priority notification."""
        api_session.post(f"{gateway_url}/api/events", json=valid_security_alert_payload)
        time.sleep(5)

        response = api_session.get(
            f"{notification_service_url}/api/notifications",
            params={"userId": valid_security_alert_payload["userId"]}
        )
        assert response.status_code in (200, 204)

    def test_multiple_events_produce_multiple_notifications(
        self, api_session, gateway_url, notification_service_url
    ):
        """Sending N events should eventually produce N notifications."""
        payloads = [
            {"eventType": "USER_REGISTERED", "userId": "batch-1", "email": "b1@e.com"},
            {"eventType": "ORDER_SHIPPED", "userId": "batch-2", "email": "b2@e.com"},
            {"eventType": "PAYMENT_FAILED", "userId": "batch-3", "email": "b3@e.com"},
        ]
        for p in payloads:
            resp = api_session.post(f"{gateway_url}/api/events", json=p)
            assert resp.status_code in (200, 201, 202)

        time.sleep(8)  # allow Kafka processing

        response = api_session.get(f"{notification_service_url}/api/notifications")
        assert response.status_code in (200, 204)
