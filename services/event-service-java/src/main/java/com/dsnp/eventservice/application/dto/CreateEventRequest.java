package com.dsnp.eventservice.application.dto;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;

/**
 * Inbound DTO for event creation requests.
 * <p>
 * Validated at the API layer via Jakarta Bean Validation annotations.
 * </p>
 */
public record CreateEventRequest(

        @NotBlank(message = "eventType is required")
        String eventType,

        @NotBlank(message = "userId is required")
        String userId,

        @NotBlank(message = "email is required")
        @Email(message = "email must be a valid email address")
        String email,

        String timestamp
) {}
