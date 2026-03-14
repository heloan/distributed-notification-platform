using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Exceptions;

/// <summary>
/// Thrown when no <see cref="Entities.NotificationRule"/> exists for a given event type.
/// </summary>
public sealed class RuleNotFoundException : Exception
{
    /// <summary>The event type that had no matching rule.</summary>
    public EventType EventType { get; }

    /// <summary>
    /// Creates a new <see cref="RuleNotFoundException"/>.
    /// </summary>
    public RuleNotFoundException(EventType eventType)
        : base($"No notification rule found for event type '{eventType}'.")
    {
        EventType = eventType;
    }
}
