package com.dsnp.eventservice;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * Entry point for the DSNP Event Service.
 * <p>
 * Spring Boot auto-configures:
 * - Web server (Tomcat)
 * - JPA / Hibernate
 * - Kafka producer
 * - Actuator endpoints
 * - Prometheus metrics
 * - OpenAPI documentation
 * </p>
 */
@SpringBootApplication
public class EventServiceApplication {

    public static void main(String[] args) {
        SpringApplication.run(EventServiceApplication.class, args);
    }
}
