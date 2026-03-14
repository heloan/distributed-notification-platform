package com.dsnp.eventservice.application.mapper;

import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.domain.entity.Event;

/**
 * Maps between domain entities and application DTOs.
 * <p>
 * Pure function — no framework dependency. Follows Single Responsibility.
 * </p>
 */
public final class EventMapper {

    private EventMapper() {
        // Utility class — prevent instantiation
    }

    /**
     * Convert a domain Event to an EventResponse DTO.
     */
    public static EventResponse toResponse(Event event) {
        return new EventResponse(
                event.getId(),
                event.getEventType().name(),
                event.getUserId(),
                event.getEmail(),
                event.getCreatedAt(),
                "CREATED"
        );
    }
}
