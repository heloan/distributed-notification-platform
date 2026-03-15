namespace NotificationService.Domain.Enums;

/// <summary>
/// Notification delivery channels supported by the platform.
/// </summary>
public enum ChannelType
{
    /// <summary>Email notification.</summary>
    Email,

    /// <summary>Slack message.</summary>
    Slack,

    /// <summary>SMS text message.</summary>
    Sms,

    /// <summary>Push notification.</summary>
    Push
}
