using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

/// <summary>
/// Aggregate root representing a notification dispatched (or to be dispatched)
/// through a specific channel for a given event.
/// </summary>
public sealed class Notification
{
    // -----------------------------------------------------------------------
    // Properties
    // -----------------------------------------------------------------------

    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; }

    /// <summary>Foreign key — the event that triggered this notification.</summary>
    public Guid EventId { get; private set; }

    /// <summary>Delivery channel (Email, Slack, SMS).</summary>
    public ChannelType Channel { get; private set; }

    /// <summary>Recipient address (email, phone number, Slack ID).</summary>
    public string Recipient { get; private set; } = string.Empty;

    /// <summary>Current delivery status.</summary>
    public NotificationStatus Status { get; private set; }

    /// <summary>Notification body / content.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>Timestamp when the notification was successfully sent.</summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>Timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; private set; }

    // -----------------------------------------------------------------------
    // Factory — Create
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Notification"/> with PENDING status.
    /// </summary>
    public static Notification Create(
        Guid eventId,
        ChannelType channel,
        string recipient,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient, nameof(recipient));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        return new Notification
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Channel = channel,
            Recipient = recipient,
            Status = NotificationStatus.Pending,
            Message = message,
            SentAt = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    // -----------------------------------------------------------------------
    // Behaviour — State Transitions
    // -----------------------------------------------------------------------

    /// <summary>
    /// Marks the notification as successfully sent.
    /// </summary>
    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the notification as failed.
    /// </summary>
    public void MarkAsFailed()
    {
        Status = NotificationStatus.Failed;
    }

    /// <summary>
    /// Marks the notification for retry.
    /// </summary>
    public void MarkAsRetrying()
    {
        Status = NotificationStatus.Retrying;
    }

    // -----------------------------------------------------------------------
    // Identity equality
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Notification other && Id == other.Id;

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString() =>
        $"Notification {{ Id={Id}, EventId={EventId}, Channel={Channel}, Status={Status} }}";
}
