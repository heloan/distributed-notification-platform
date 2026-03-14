package com.dsnp.eventservice.application.port;

import com.dsnp.eventservice.domain.event.EventCreatedDomainEvent;

/**
 * Output port for publishing domain events to the message broker.
 * <p>
 * Defined in the Application layer — implemented in Infrastructure (Kafka).
 * Follows the Dependency Inversion Principle (DIP).
 * </p>
 */
public interface EventPublisher {

    /**
     * Publish an event-created domain event to the message broker.
     *
     * @param domainEvent the domain event to publish
     */
    void publish(EventCreatedDomainEvent domainEvent);
}
