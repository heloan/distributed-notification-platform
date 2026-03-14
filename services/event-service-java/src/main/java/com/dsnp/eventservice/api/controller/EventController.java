package com.dsnp.eventservice.api.controller;

import java.util.List;
import java.util.UUID;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;

import com.dsnp.eventservice.application.dto.CreateEventRequest;
import com.dsnp.eventservice.application.dto.EventResponse;
import com.dsnp.eventservice.application.usecase.CreateEventUseCase;
import com.dsnp.eventservice.application.usecase.GetAllEventsUseCase;
import com.dsnp.eventservice.application.usecase.GetEventByIdUseCase;

import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;

/**
 * REST controller for event operations.
 * <p>
 * Thin adapter — delegates all business logic to use cases.
 * Follows the Interface Segregation Principle (ISP): one controller per aggregate.
 * </p>
 */
@RestController
@RequestMapping("/api/events")
@Tag(name = "Events", description = "Event ingestion and retrieval")
public class EventController {

    private final CreateEventUseCase createEventUseCase;
    private final GetEventByIdUseCase getEventByIdUseCase;
    private final GetAllEventsUseCase getAllEventsUseCase;

    public EventController(
            CreateEventUseCase createEventUseCase,
            GetEventByIdUseCase getEventByIdUseCase,
            GetAllEventsUseCase getAllEventsUseCase
    ) {
        this.createEventUseCase = createEventUseCase;
        this.getEventByIdUseCase = getEventByIdUseCase;
        this.getAllEventsUseCase = getAllEventsUseCase;
    }

    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    @Operation(summary = "Create a new event", description = "Validates, persists, and publishes an event to Kafka")
    @ApiResponse(responseCode = "201", description = "Event created successfully")
    @ApiResponse(responseCode = "400", description = "Validation error")
    public ResponseEntity<EventResponse> createEvent(@Valid @RequestBody CreateEventRequest request) {
        EventResponse response = createEventUseCase.execute(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(response);
    }

    @GetMapping("/{id}")
    @Operation(summary = "Get event by ID", description = "Retrieve a single event by its UUID")
    @ApiResponse(responseCode = "200", description = "Event found")
    @ApiResponse(responseCode = "404", description = "Event not found")
    public ResponseEntity<EventResponse> getEventById(@PathVariable UUID id) {
        EventResponse response = getEventByIdUseCase.execute(id);
        return ResponseEntity.ok(response);
    }

    @GetMapping
    @Operation(summary = "Get all events", description = "Retrieve all events ordered by creation date (newest first)")
    @ApiResponse(responseCode = "200", description = "List of events")
    public ResponseEntity<List<EventResponse>> getAllEvents() {
        List<EventResponse> events = getAllEventsUseCase.execute();
        return ResponseEntity.ok(events);
    }
}
