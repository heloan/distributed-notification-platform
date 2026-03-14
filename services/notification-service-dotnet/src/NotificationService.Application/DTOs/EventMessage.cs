namespace NotificationService.Application.DTOs;

/// <summary>
/// Represents a Kafka event payload consumed from the <c>events</c> topic.
/// </summary>
public sealed record EventMessage(
    string EventId,
    string EventType,
    string UserId,
    string? Email,
    DateTime? OccurredAt
);
