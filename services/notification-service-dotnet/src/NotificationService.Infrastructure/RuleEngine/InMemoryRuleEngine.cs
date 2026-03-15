using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.RuleEngine;

/// <summary>
/// In-memory implementation of <see cref="IRuleEngine"/>.
/// Maps each <see cref="EventType"/> to channels and a message template.
///
/// Open/Closed Principle — new rules are added here without modifying use cases.
/// </summary>
public sealed class InMemoryRuleEngine : IRuleEngine
{
    private static readonly Dictionary<EventType, NotificationRule> Rules = new()
    {
        [EventType.UserRegistered] = new NotificationRule(
            EventType.UserRegistered,
            new[] { ChannelType.Email },
            "Welcome to the platform! Your account has been successfully created."),

        [EventType.PaymentFailed] = new NotificationRule(
            EventType.PaymentFailed,
            new[] { ChannelType.Sms },
            "⚠️ Payment processing failed. Please review the transaction details."),

        [EventType.OrderShipped] = new NotificationRule(
            EventType.OrderShipped,
            new[] { ChannelType.Sms },
            "Your order has been shipped! Track your delivery for updates."),

        [EventType.SecurityAlert] = new NotificationRule(
            EventType.SecurityAlert,
            new[] { ChannelType.Email, ChannelType.Slack, ChannelType.Push },
            "🔒 Security alert: Suspicious activity detected on your account. Please verify your recent actions.")
    };

    /// <inheritdoc />
    public NotificationRule? Evaluate(EventType eventType)
    {
        return Rules.TryGetValue(eventType, out var rule) ? rule : null;
    }
}
