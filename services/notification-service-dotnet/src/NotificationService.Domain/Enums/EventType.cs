namespace NotificationService.Domain.Enums;

/// <summary>
/// Types of platform events that trigger notifications.
/// </summary>
public enum EventType
{
    /// <summary>New user sign-up.</summary>
    UserRegistered,

    /// <summary>Payment processing failure.</summary>
    PaymentFailed,

    /// <summary>Order dispatched to shipping carrier.</summary>
    OrderShipped,

    /// <summary>Suspicious or unauthorized activity detected.</summary>
    SecurityAlert
}
