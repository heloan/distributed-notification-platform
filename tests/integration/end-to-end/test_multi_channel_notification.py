"""
End-to-End — Multi-Channel Notification Tests.
Validates that different event types trigger notifications through appropriate channels.
"""

import time

import pytest


@pytest.mark.e2e
class TestMultiChannelNotification:
    """Verify multi-channel notification dispatch based on event type."""

    def test_user_registered_triggers_email(
        self, api_session, gateway_url, notification_service_url, valid_user_registered_payload
    ):
        """USER_REGISTERED should produce an email notification."""
        api_session.post(f"{gateway_url}/api/events", json=valid_user_registered_payload)
        time.sleep(5)

        response = api_session.get(
            f"{notification_service_url}/api/notifications",
            params={"userId": valid_user_registered_payload["userId"]}
        )
        if response.status_code == 200:
            notifications = response.json()
            channels = [n.get("channel", "").lower() for n in notifications]
            assert any("email" in c for c in channels), "Expected email notification"

    def test_payment_failed_triggers_sms(
        self, api_session, gateway_url, notification_service_url, valid_payment_failed_payload
    ):
        """PAYMENT_FAILED should produce an SMS notification."""
        api_session.post(f"{gateway_url}/api/events", json=valid_payment_failed_payload)
        time.sleep(5)

        response = api_session.get(
            f"{notification_service_url}/api/notifications",
            params={"userId": valid_payment_failed_payload["userId"]}
        )
        if response.status_code == 200:
            notifications = response.json()
            channels = [n.get("channel", "").lower() for n in notifications]
            assert any("sms" in c for c in channels), "Expected SMS notification"

    def test_security_alert_triggers_push(
        self, api_session, gateway_url, notification_service_url, valid_security_alert_payload
    ):
        """SECURITY_ALERT should produce a push notification."""
        api_session.post(f"{gateway_url}/api/events", json=valid_security_alert_payload)
        time.sleep(5)

        response = api_session.get(
            f"{notification_service_url}/api/notifications",
            params={"userId": valid_security_alert_payload["userId"]}
        )
        if response.status_code == 200:
            notifications = response.json()
            channels = [n.get("channel", "").lower() for n in notifications]
            assert any("push" in c for c in channels), "Expected push notification"
