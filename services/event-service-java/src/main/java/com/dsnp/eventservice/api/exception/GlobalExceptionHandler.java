package com.dsnp.eventservice.api.exception;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import com.dsnp.eventservice.application.dto.ErrorResponse;
import com.dsnp.eventservice.domain.exception.EventNotFoundException;
import com.dsnp.eventservice.domain.exception.EventValidationException;

/**
 * Global exception handler for the REST API.
 * <p>
 * Translates domain and validation exceptions into consistent JSON error responses.
 * Single Responsibility — handles all error mapping in one place.
 * </p>
 */
@RestControllerAdvice
public class GlobalExceptionHandler {

    private static final Logger log = LoggerFactory.getLogger(GlobalExceptionHandler.class);

    /**
     * Handle Jakarta Bean Validation errors (e.g., @NotBlank, @Email).
     */
    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ErrorResponse> handleValidation(MethodArgumentNotValidException ex) {
        List<String> details = ex.getBindingResult().getFieldErrors()
                .stream()
                .map(error -> "%s: %s".formatted(error.getField(), error.getDefaultMessage()))
                .toList();

        log.warn("Validation failed: {}", details);

        return ResponseEntity.badRequest().body(
                ErrorResponse.of(400, "Validation Error", "Request validation failed", details)
        );
    }

    /**
     * Handle unsupported event type from domain.
     */
    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<ErrorResponse> handleIllegalArgument(IllegalArgumentException ex) {
        log.warn("Bad request: {}", ex.getMessage());

        return ResponseEntity.badRequest().body(
                ErrorResponse.of(400, "Bad Request", ex.getMessage())
        );
    }

    /**
     * Handle domain validation errors.
     */
    @ExceptionHandler(EventValidationException.class)
    public ResponseEntity<ErrorResponse> handleEventValidation(EventValidationException ex) {
        log.warn("Event validation failed: {}", ex.getMessage());

        return ResponseEntity.badRequest().body(
                ErrorResponse.of(400, "Validation Error", ex.getMessage())
        );
    }

    /**
     * Handle event not found.
     */
    @ExceptionHandler(EventNotFoundException.class)
    public ResponseEntity<ErrorResponse> handleEventNotFound(EventNotFoundException ex) {
        log.info("Event not found: {}", ex.getEventId());

        return ResponseEntity.status(HttpStatus.NOT_FOUND).body(
                ErrorResponse.of(404, "Not Found", ex.getMessage())
        );
    }

    /**
     * Catch-all for unexpected errors.
     */
    @ExceptionHandler(Exception.class)
    public ResponseEntity<ErrorResponse> handleUnexpected(Exception ex) {
        log.error("Unexpected error: {}", ex.getMessage(), ex);

        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(
                ErrorResponse.of(500, "Internal Server Error", "An unexpected error occurred")
        );
    }
}
