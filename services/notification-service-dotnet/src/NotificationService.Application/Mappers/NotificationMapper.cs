using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Mappers;

/// <summary>
/// Maps between <see cref="Notification"/> domain entities and outbound DTOs.
/// </summary>
public static class NotificationMapper
{
    /// <summary>
    /// Maps a domain <see cref="Notification"/> to a <see cref="NotificationResponse"/> DTO.
    /// </summary>
    public static NotificationResponse ToResponse(Notification notification) =>
        new(
            Id: notification.Id,
            EventId: notification.EventId,
            Channel: notification.Channel.ToString().ToUpperInvariant(),
            Recipient: notification.Recipient,
            Status: notification.Status.ToString().ToUpperInvariant(),
            Message: notification.Message,
            SentAt: notification.SentAt,
            CreatedAt: notification.CreatedAt
        );
}
