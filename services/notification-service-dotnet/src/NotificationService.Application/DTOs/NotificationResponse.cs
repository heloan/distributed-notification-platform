using NotificationService.Domain.Enums;

namespace NotificationService.Application.DTOs;

/// <summary>
/// Outbound DTO representing a persisted notification record.
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    Guid EventId,
    string Channel,
    string Recipient,
    string Status,
    string Message,
    DateTime? SentAt,
    DateTime CreatedAt
);
