package com.dsnp.eventservice.domain.repository;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

import com.dsnp.eventservice.domain.entity.Event;

/**
 * Repository contract for {@link Event} persistence.
 * <p>
 * Defined in the Domain layer — implemented in Infrastructure.
 * Follows the Dependency Inversion Principle (DIP).
 * </p>
 */
public interface EventRepository {

    /**
     * Persist a new event.
     *
     * @param event the domain event to save
     * @return the persisted event with generated ID
     */
    Event save(Event event);

    /**
     * Find an event by its unique identifier.
     *
     * @param id the event UUID
     * @return an Optional containing the event, or empty if not found
     */
    Optional<Event> findById(UUID id);

    /**
     * Retrieve all events ordered by creation date (newest first).
     *
     * @return list of all events
     */
    List<Event> findAllOrderByCreatedAtDesc();
}
