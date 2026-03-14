package com.dsnp.eventservice.application.dto;

import java.time.Instant;
import java.util.UUID;

/**
 * Outbound DTO returned after event creation or retrieval.
 */
public record EventResponse(
        UUID id,
        String eventType,
        String userId,
        String email,
        Instant createdAt,
        String status
) {}
