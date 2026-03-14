package com.dsnp.eventservice.infrastructure.persistence;

import java.time.Instant;
import java.util.UUID;

import com.dsnp.eventservice.domain.entity.Event;
import com.dsnp.eventservice.domain.enums.EventType;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

/**
 * JPA entity mapped to the {@code events} PostgreSQL table.
 * <p>
 * This is an Infrastructure concern — it converts between the database
 * representation and the domain {@link Event} entity.
 * </p>
 */
@Entity
@Table(name = "events")
public class EventJpaEntity {

    @Id
    @Column(name = "id", nullable = false, updatable = false)
    private UUID id;

    @Column(name = "event_type", nullable = false, length = 100)
    @Enumerated(EnumType.STRING)
    private EventType eventType;

    @Column(name = "user_id", nullable = false, length = 255)
    private String userId;

    @Column(name = "payload", nullable = false, columnDefinition = "jsonb")
    private String payload;

    @Column(name = "created_at", nullable = false, updatable = false)
    private Instant createdAt;

    // -----------------------------------------------------------------------
    // Domain ↔ JPA mapping
    // -----------------------------------------------------------------------

    /**
     * Convert a domain Event to a JPA entity.
     */
    public static EventJpaEntity fromDomain(Event event) {
        var entity = new EventJpaEntity();
        entity.id = event.getId();
        entity.eventType = event.getEventType();
        entity.userId = event.getUserId();
        entity.payload = event.getPayload();
        entity.createdAt = event.getCreatedAt();
        return entity;
    }

    /**
     * Convert this JPA entity back to a domain Event.
     */
    public Event toDomain() {
        // Extract email from payload — stored in JSONB
        String email = extractEmailFromPayload();
        return Event.reconstitute(id, eventType, userId, email, payload, createdAt);
    }

    private String extractEmailFromPayload() {
        // Simple extraction — in production, use Jackson ObjectMapper
        if (payload != null && payload.contains("\"email\":\"")) {
            int start = payload.indexOf("\"email\":\"") + 9;
            int end = payload.indexOf("\"", start);
            if (end > start) {
                return payload.substring(start, end);
            }
        }
        return "";
    }

    // -----------------------------------------------------------------------
    // Getters (for Spring Data queries)
    // -----------------------------------------------------------------------

    public UUID getId() { return id; }
    public EventType getEventType() { return eventType; }
    public String getUserId() { return userId; }
    public String getPayload() { return payload; }
    public Instant getCreatedAt() { return createdAt; }

    // JPA requires no-arg constructor
    protected EventJpaEntity() {}
}
