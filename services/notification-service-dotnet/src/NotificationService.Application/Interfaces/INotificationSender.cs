using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Interfaces;

/// <summary>
/// Notification dispatch port — sends a notification through a specific channel.
/// Each channel provider (Email, Slack, SMS) implements this interface.
/// Follows Liskov Substitution — all providers are interchangeable.
/// </summary>
public interface INotificationSender
{
    /// <summary>The channel this sender supports.</summary>
    ChannelType Channel { get; }

    /// <summary>
    /// Dispatches the notification to its recipient.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if delivery succeeded; <c>false</c> otherwise.</returns>
    Task<bool> SendAsync(Notification notification, CancellationToken cancellationToken = default);
}
