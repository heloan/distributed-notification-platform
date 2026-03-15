-- =============================================================================
-- Distributed Smart Notification Platform
-- Database Initialization Script
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ---------------------------------------------------------------------------
-- Events table
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS events (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type  VARCHAR(100)    NOT NULL,
    user_id     VARCHAR(255)    NOT NULL,
    payload     JSONB           NOT NULL,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_events_event_type ON events (event_type);
CREATE INDEX IF NOT EXISTS idx_events_created_at ON events (created_at);
CREATE INDEX IF NOT EXISTS idx_events_user_id    ON events (user_id);

-- ---------------------------------------------------------------------------
-- Notifications table
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS notifications (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id    UUID            NOT NULL REFERENCES events(id),
    user_id     VARCHAR(255),
    channel     VARCHAR(50)     NOT NULL,
    recipient   VARCHAR(255)    NOT NULL,
    status      VARCHAR(50)     NOT NULL DEFAULT 'PENDING',
    message     TEXT,
    sent_at     TIMESTAMPTZ,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_notifications_event_id ON notifications (event_id);
CREATE INDEX IF NOT EXISTS idx_notifications_status   ON notifications (status);
CREATE INDEX IF NOT EXISTS idx_notifications_channel  ON notifications (channel);
CREATE INDEX IF NOT EXISTS idx_notifications_user_id  ON notifications (user_id);
