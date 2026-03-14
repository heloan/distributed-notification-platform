package com.dsnp.eventservice.infrastructure.persistence;

import java.util.List;
import java.util.UUID;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.stereotype.Repository;

/**
 * Spring Data JPA repository for {@link EventJpaEntity}.
 * <p>
 * This interface is an Infrastructure detail — it is NOT exposed to
 * the Application layer directly. The adapter {@link JpaEventRepositoryAdapter}
 * wraps it behind the domain {@code EventRepository} contract.
 * </p>
 */
@Repository
public interface SpringDataEventRepository extends JpaRepository<EventJpaEntity, UUID> {

    @Query("SELECT e FROM EventJpaEntity e ORDER BY e.createdAt DESC")
    List<EventJpaEntity> findAllOrderByCreatedAtDesc();
}
