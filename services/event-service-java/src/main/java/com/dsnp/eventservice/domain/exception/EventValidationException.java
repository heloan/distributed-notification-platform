package com.dsnp.eventservice.domain.exception;

/**
 * Thrown when event validation fails at the domain level.
 */
public class EventValidationException extends RuntimeException {

    public EventValidationException(String message) {
        super(message);
    }
}
