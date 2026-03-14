package com.dsnp.eventservice.domain.event;

import java.time.Instant;
import java.util.UUID;

import com.dsnp.eventservice.domain.enums.EventType;

/**
 * Domain event raised when a new application event is created.
 * <p>
 * Published to the message broker (Kafka) by the Infrastructure layer.
 * This is a record — immutable by design.
 * </p>
 */
public record EventCreatedDomainEvent(
        UUID eventId,
        EventType eventType,
        String userId,
        String email,
        String payload,
        Instant createdAt
) {
    /**
     * Create from a domain Event entity.
     */
    public static EventCreatedDomainEvent from(com.dsnp.eventservice.domain.entity.Event event) {
        return new EventCreatedDomainEvent(
                event.getId(),
                event.getEventType(),
                event.getUserId(),
                event.getEmail(),
                event.getPayload(),
                event.getCreatedAt()
        );
    }
}
