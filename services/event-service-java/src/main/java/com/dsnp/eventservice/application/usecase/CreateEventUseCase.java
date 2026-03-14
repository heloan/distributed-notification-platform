package com.dsnp.eventservice.application.usecase;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.dsnp.eventservice.application.dto.CreateEventRequest;
import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.application.mapper.EventMapper;
import com.dsnp.eventservice.application.port.EventPublisher;
import com.dsnp.eventservice.domain.entity.Event;
import com.dsnp.eventservice.domain.enums.EventType;
import com.dsnp.eventservice.domain.event.EventCreatedDomainEvent;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Use case: Create a new application event.
 * <p>
 * Orchestrates: validation → persistence → publish domain event.
 * Single Responsibility — this class does one thing only.
 * </p>
 */
@Service
public class CreateEventUseCase {

    private static final Logger log = LoggerFactory.getLogger(CreateEventUseCase.class);

    private final EventRepository eventRepository;
    private final EventPublisher eventPublisher;

    public CreateEventUseCase(EventRepository eventRepository, EventPublisher eventPublisher) {
        this.eventRepository = eventRepository;
        this.eventPublisher = eventPublisher;
    }

    /**
     * Execute the use case.
     *
     * @param request the inbound event creation request
     * @return the created event response
     */
    @Transactional
    public EventResponse execute(CreateEventRequest request) {
        log.info("Creating event: type={}, userId={}", request.eventType(), request.userId());

        // 1. Parse and validate the event type (domain validation)
        EventType eventType = EventType.fromString(request.eventType());

        // 2. Build the JSON payload
        String payload = buildPayload(request);

        // 3. Create the domain entity
        Event event = Event.create(eventType, request.userId(), request.email(), payload);

        // 4. Persist to database
        Event saved = eventRepository.save(event);
        log.info("Event persisted: id={}", saved.getId());

        // 5. Publish domain event to Kafka
        EventCreatedDomainEvent domainEvent = EventCreatedDomainEvent.from(saved);
        eventPublisher.publish(domainEvent);
        log.info("Event published to broker: id={}", saved.getId());

        // 6. Map to response DTO
        return EventMapper.toResponse(saved);
    }

    private String buildPayload(CreateEventRequest request) {
        return """
                {"eventType":"%s","userId":"%s","email":"%s","timestamp":"%s"}"""
                .formatted(
                        request.eventType(),
                        request.userId(),
                        request.email(),
                        request.timestamp() != null ? request.timestamp() : ""
                );
    }
}
