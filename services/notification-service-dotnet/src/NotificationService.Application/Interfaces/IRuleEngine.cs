using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Interfaces;

/// <summary>
/// Rule engine port — evaluates an event type and returns the matching notification rule.
/// Follows Interface Segregation Principle (ISP).
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Evaluates which notification rule(s) apply for the given event type.
    /// </summary>
    /// <param name="eventType">The event type to evaluate.</param>
    /// <returns>The matching <see cref="NotificationRule"/>, or <c>null</c> if none.</returns>
    NotificationRule? Evaluate(EventType eventType);
}
