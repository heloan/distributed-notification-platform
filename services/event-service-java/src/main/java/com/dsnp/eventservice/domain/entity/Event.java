package com.dsnp.eventservice.domain.entity;

import java.time.Instant;
import java.util.Objects;
import java.util.UUID;

import com.dsnp.eventservice.domain.enums.EventType;

/**
 * Domain entity representing an application event.
 * <p>
 * This is a rich domain model — it encapsulates state and behaviour.
 * No framework annotations here; mapping to persistence is in the Infrastructure layer.
 * </p>
 */
public class Event {

    private UUID id;
    private EventType eventType;
    private String userId;
    private String email;
    private String payload;
    private Instant createdAt;

    // -----------------------------------------------------------------------
    // Factory method — preferred over constructor for clarity
    // -----------------------------------------------------------------------

    /**
     * Create a new Event with a generated ID and current timestamp.
     */
    public static Event create(EventType eventType, String userId, String email, String payload) {
        Objects.requireNonNull(eventType, "eventType must not be null");
        Objects.requireNonNull(userId, "userId must not be null");
        Objects.requireNonNull(email, "email must not be null");

        var event = new Event();
        event.id = UUID.randomUUID();
        event.eventType = eventType;
        event.userId = userId;
        event.email = email;
        event.payload = payload;
        event.createdAt = Instant.now();
        return event;
    }

    // -----------------------------------------------------------------------
    // Reconstitution — used when loading from persistence
    // -----------------------------------------------------------------------

    /**
     * Reconstitute an Event from persisted state (no validation, trusted data).
     */
    public static Event reconstitute(UUID id, EventType eventType, String userId,
                                     String email, String payload, Instant createdAt) {
        var event = new Event();
        event.id = id;
        event.eventType = eventType;
        event.userId = userId;
        event.email = email;
        event.payload = payload;
        event.createdAt = createdAt;
        return event;
    }

    // -----------------------------------------------------------------------
    // Getters — no setters (immutable after creation)
    // -----------------------------------------------------------------------

    public UUID getId() { return id; }
    public EventType getEventType() { return eventType; }
    public String getUserId() { return userId; }
    public String getEmail() { return email; }
    public String getPayload() { return payload; }
    public Instant getCreatedAt() { return createdAt; }

    // -----------------------------------------------------------------------
    // Equals / HashCode — identity by ID
    // -----------------------------------------------------------------------

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Event event = (Event) o;
        return Objects.equals(id, event.id);
    }

    @Override
    public int hashCode() {
        return Objects.hash(id);
    }

    @Override
    public String toString() {
        return "Event{id=%s, eventType=%s, userId='%s', createdAt=%s}"
                .formatted(id, eventType, userId, createdAt);
    }

    // Protected no-arg constructor for JPA proxy (Infrastructure concern leak kept minimal)
    protected Event() {}
}
