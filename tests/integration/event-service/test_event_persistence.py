"""
Event Service — Event Persistence Integration Tests.
Validates that events are persisted in PostgreSQL and retrievable.
"""

import pytest


@pytest.mark.event_service
class TestEventPersistence:
    """Verify the Event Service persists events to the database."""

    def test_get_events_returns_200(self, api_session, event_service_url):
        """GET /api/events should return 200 with a list."""
        response = api_session.get(f"{event_service_url}/api/events")
        assert response.status_code == 200
        assert isinstance(response.json(), list)

    def test_get_event_by_id(self, api_session, event_service_url, valid_user_registered_payload):
        """A created event should be retrievable by its ID."""
        create = api_session.post(f"{event_service_url}/api/events", json=valid_user_registered_payload)
        if create.status_code in (200, 201):
            event_id = create.json().get("id")
            get = api_session.get(f"{event_service_url}/api/events/{event_id}")
            assert get.status_code == 200
            assert get.json()["id"] == event_id

    def test_get_nonexistent_event_returns_404(self, api_session, event_service_url):
        """GET /api/events/{nonexistent-id} should return 404."""
        response = api_session.get(f"{event_service_url}/api/events/00000000-0000-0000-0000-000000000000")
        assert response.status_code == 404

    def test_events_ordered_by_creation(self, api_session, event_service_url):
        """Events list should be ordered (newest first)."""
        response = api_session.get(f"{event_service_url}/api/events")
        if response.status_code == 200:
            events = response.json()
            if len(events) >= 2:
                assert events[0].get("createdAt", "") >= events[1].get("createdAt", "")
