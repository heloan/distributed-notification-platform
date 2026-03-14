package com.dsnp.eventservice.infrastructure.persistence;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

import org.springframework.stereotype.Component;

import com.dsnp.eventservice.domain.entity.Event;
import com.dsnp.eventservice.domain.repository.EventRepository;

/**
 * Adapter that implements the domain {@link EventRepository} using Spring Data JPA.
 * <p>
 * This is the bridge between the domain contract and the Spring infrastructure.
 * Follows the Adapter pattern from Hexagonal Architecture.
 * </p>
 */
@Component
public class JpaEventRepositoryAdapter implements EventRepository {

    private final SpringDataEventRepository jpaRepository;

    public JpaEventRepositoryAdapter(SpringDataEventRepository jpaRepository) {
        this.jpaRepository = jpaRepository;
    }

    @Override
    public Event save(Event event) {
        EventJpaEntity entity = EventJpaEntity.fromDomain(event);
        EventJpaEntity saved = jpaRepository.save(entity);
        return saved.toDomain();
    }

    @Override
    public Optional<Event> findById(UUID id) {
        return jpaRepository.findById(id)
                .map(EventJpaEntity::toDomain);
    }

    @Override
    public List<Event> findAllOrderByCreatedAtDesc() {
        return jpaRepository.findAllOrderByCreatedAtDesc()
                .stream()
                .map(EventJpaEntity::toDomain)
                .toList();
    }
}
