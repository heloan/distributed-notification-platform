namespace NotificationService.Domain.Exceptions;

/// <summary>
/// Base exception for domain-level rule violations within the Notification Service.
/// </summary>
public sealed class NotificationDomainException : Exception
{
    /// <summary>
    /// Creates a new <see cref="NotificationDomainException"/> with the given message.
    /// </summary>
    public NotificationDomainException(string message) : base(message) { }

    /// <summary>
    /// Creates a new <see cref="NotificationDomainException"/> wrapping an inner exception.
    /// </summary>
    public NotificationDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
