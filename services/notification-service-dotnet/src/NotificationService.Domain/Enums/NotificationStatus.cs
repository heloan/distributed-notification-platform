namespace NotificationService.Domain.Enums;

/// <summary>
/// Lifecycle states of a notification.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Notification created, not yet dispatched.</summary>
    Pending,

    /// <summary>Successfully delivered through the channel provider.</summary>
    Sent,

    /// <summary>Delivery attempt failed.</summary>
    Failed,

    /// <summary>Scheduled for retry after a transient failure.</summary>
    Retrying
}
