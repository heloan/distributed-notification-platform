# 🗄️ Database Design

## Overview

The system uses **PostgreSQL** as the primary relational database. Two main tables store the event history and notification records.

---

## Entity-Relationship Diagram

```
┌──────────────────────────┐
│         events           │
├──────────────────────────┤
│ id          UUID    [PK] │
│ event_type  VARCHAR      │
│ user_id     VARCHAR      │
│ payload     JSONB        │
│ created_at  TIMESTAMPTZ  │
└────────────┬─────────────┘
             │
             │ 1
             │
             │ *
┌────────────▼─────────────┐
│      notifications       │
├──────────────────────────┤
│ id          UUID    [PK] │
│ event_id    UUID    [FK] │
│ channel     VARCHAR      │
│ recipient   VARCHAR      │
│ status      VARCHAR      │
│ message     TEXT         │
│ sent_at     TIMESTAMPTZ  │
│ created_at  TIMESTAMPTZ  │
└──────────────────────────┘
```

**Relationship:** One event can trigger multiple notifications (one-to-many).

---

## Table Definitions

### `events`

Stores all ingested application events.

| Column | Type | Constraints | Description |
|--------|------|------------|-------------|
| `id` | `UUID` | `PRIMARY KEY`, `DEFAULT gen_random_uuid()` | Unique event identifier |
| `event_type` | `VARCHAR(100)` | `NOT NULL` | Type of event (e.g., `USER_REGISTERED`) |
| `user_id` | `VARCHAR(255)` | `NOT NULL` | ID of the user associated with the event |
| `payload` | `JSONB` | `NOT NULL` | Full event data as JSON |
| `created_at` | `TIMESTAMPTZ` | `NOT NULL`, `DEFAULT NOW()` | Timestamp when the event was received |

**Indexes:**

```sql
CREATE INDEX idx_events_event_type ON events (event_type);
CREATE INDEX idx_events_created_at ON events (created_at);
CREATE INDEX idx_events_user_id ON events (user_id);
```

### `notifications`

Stores the notification history for every dispatched notification.

| Column | Type | Constraints | Description |
|--------|------|------------|-------------|
| `id` | `UUID` | `PRIMARY KEY`, `DEFAULT gen_random_uuid()` | Unique notification identifier |
| `event_id` | `UUID` | `NOT NULL`, `REFERENCES events(id)` | Foreign key to the triggering event |
| `channel` | `VARCHAR(50)` | `NOT NULL` | Notification channel (`EMAIL`, `SLACK`, `SMS`) |
| `recipient` | `VARCHAR(255)` | `NOT NULL` | Recipient address (email, phone, Slack ID) |
| `status` | `VARCHAR(50)` | `NOT NULL`, `DEFAULT 'PENDING'` | Delivery status |
| `message` | `TEXT` | | Notification content |
| `sent_at` | `TIMESTAMPTZ` | | Timestamp when notification was sent |
| `created_at` | `TIMESTAMPTZ` | `NOT NULL`, `DEFAULT NOW()` | Timestamp when record was created |

**Indexes:**

```sql
CREATE INDEX idx_notifications_event_id ON notifications (event_id);
CREATE INDEX idx_notifications_status ON notifications (status);
CREATE INDEX idx_notifications_channel ON notifications (channel);
```

---

## Notification Statuses

| Status | Description |
|--------|-------------|
| `PENDING` | Notification created, not yet sent |
| `SENT` | Successfully dispatched |
| `FAILED` | Delivery failed |
| `RETRYING` | Scheduled for retry |

---

## Channel Types

| Channel | Description | Recipient Format |
|---------|-------------|-----------------|
| `EMAIL` | Email notification | Email address |
| `SLACK` | Slack message | Slack channel/user ID |
| `SMS` | Text message | Phone number |

---

## Sample Data

### Event

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "event_type": "USER_REGISTERED",
  "user_id": "123",
  "payload": {
    "eventType": "USER_REGISTERED",
    "userId": "123",
    "email": "user@email.com",
    "timestamp": "2026-03-14T10:15:00Z"
  },
  "created_at": "2026-03-14T10:15:01Z"
}
```

### Notification

```json
{
  "id": "f9e8d7c6-b5a4-3210-fedc-ba0987654321",
  "event_id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "channel": "EMAIL",
  "recipient": "user@email.com",
  "status": "SENT",
  "message": "Welcome to the platform!",
  "sent_at": "2026-03-14T10:15:02Z",
  "created_at": "2026-03-14T10:15:01Z"
}
```

---

## Database Initialization Script

```sql
-- ============================================================================
-- Distributed Smart Notification Platform
-- Database Schema
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Events table
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

-- Notifications table
CREATE TABLE IF NOT EXISTS notifications (
    id          UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    event_id    UUID            NOT NULL REFERENCES events(id),
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
```

---

## Data Flow

```
Client sends event
        ↓
Event Service persists to `events` table
        ↓
Event published to Kafka
        ↓
Notification Service consumes event
        ↓
Rule Engine determines channel
        ↓
Notification created in `notifications` table (status: PENDING)
        ↓
Notification dispatched via provider
        ↓
Status updated to SENT or FAILED
```
