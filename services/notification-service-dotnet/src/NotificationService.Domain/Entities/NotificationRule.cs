using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// A rule mapping an <see cref="EventType"/> to one or more delivery channels
/// with a message template. Encapsulates the business logic for "what channel
/// should this event be routed to?"
/// </summary>
public sealed class NotificationRule
{
    /// <summary>The event type this rule applies to.</summary>
    public EventType EventType { get; }

    /// <summary>The channels to dispatch on.</summary>
    public IReadOnlyList<ChannelType> Channels { get; }

    /// <summary>Human-readable message template.</summary>
    public string MessageTemplate { get; }

    /// <summary>
    /// Creates a new <see cref="NotificationRule"/>.
    /// </summary>
    public NotificationRule(
        EventType eventType,
        IReadOnlyList<ChannelType> channels,
        string messageTemplate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageTemplate, nameof(messageTemplate));
        ArgumentNullException.ThrowIfNull(channels, nameof(channels));

        if (channels.Count == 0)
            throw new ArgumentException("At least one channel is required.", nameof(channels));

        EventType = eventType;
        Channels = channels;
        MessageTemplate = messageTemplate;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Rule {{ {EventType} → [{string.Join(", ", Channels)}] }}";
}
