package com.dsnp.eventservice.domain.exception;

/**
 * Thrown when a requested event is not found.
 */
public class EventNotFoundException extends RuntimeException {

    private final String eventId;

    public EventNotFoundException(String eventId) {
        super("Event not found: " + eventId);
        this.eventId = eventId;
    }

    public String getEventId() {
        return eventId;
    }
}
