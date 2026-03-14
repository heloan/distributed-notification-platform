using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.UseCases;

/// <summary>
/// Core business workflow — processes a consumed event:
///   1. Parse event type
///   2. Evaluate notification rules
///   3. Create Notification entities
///   4. Dispatch through the appropriate channel provider
///   5. Persist the notification with its final status
///
/// Follows Single Responsibility — only orchestrates; delegates to ports.
/// </summary>
public sealed class ProcessEventUseCase
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IEnumerable<INotificationSender> _senders;
    private readonly INotificationRepository _repository;
    private readonly ILogger<ProcessEventUseCase> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="ProcessEventUseCase"/>.
    /// </summary>
    public ProcessEventUseCase(
        IRuleEngine ruleEngine,
        IEnumerable<INotificationSender> senders,
        INotificationRepository repository,
        ILogger<ProcessEventUseCase> logger)
    {
        _ruleEngine = ruleEngine;
        _senders = senders;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Processes a single event message from the broker.
    /// </summary>
    public async Task ExecuteAsync(EventMessage message, CancellationToken cancellationToken = default)
    {
        // 1. Parse event type (normalize: USER_REGISTERED → UserRegistered)
        var normalizedType = message.EventType.Replace("_", "");
        if (!Enum.TryParse<EventType>(normalizedType, ignoreCase: true, out var eventType))
        {
            _logger.LogWarning("Unknown event type '{EventType}' — skipping", message.EventType);
            return;
        }

        if (!Guid.TryParse(message.EventId, out var eventId))
        {
            _logger.LogWarning("Invalid event ID '{EventId}' — skipping", message.EventId);
            return;
        }

        // 2. Evaluate rules
        var rule = _ruleEngine.Evaluate(eventType);
        if (rule is null)
        {
            _logger.LogInformation("No notification rule for event type '{EventType}' — skipping", eventType);
            return;
        }

        // 3. Determine recipient
        var recipient = ResolveRecipient(message, eventType);
        if (string.IsNullOrWhiteSpace(recipient))
        {
            _logger.LogWarning("Could not resolve recipient for event {EventId}", eventId);
            return;
        }

        // 4. Create & dispatch notification for each channel in the rule
        foreach (var channel in rule.Channels)
        {
            var notification = Notification.Create(
                eventId: eventId,
                channel: channel,
                recipient: recipient,
                message: rule.MessageTemplate);

            await _repository.AddAsync(notification, cancellationToken);

            var sender = _senders.FirstOrDefault(s => s.Channel == channel);
            if (sender is null)
            {
                _logger.LogError("No sender registered for channel {Channel}", channel);
                notification.MarkAsFailed();
                await _repository.UpdateAsync(notification, cancellationToken);
                continue;
            }

            try
            {
                var success = await sender.SendAsync(notification, cancellationToken);
                if (success)
                {
                    notification.MarkAsSent();
                    _logger.LogInformation(
                        "Notification {NotificationId} sent via {Channel} for event {EventId}",
                        notification.Id, channel, eventId);
                }
                else
                {
                    notification.MarkAsFailed();
                    _logger.LogWarning(
                        "Notification {NotificationId} failed via {Channel} for event {EventId}",
                        notification.Id, channel, eventId);
                }
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed();
                _logger.LogError(ex,
                    "Exception sending notification {NotificationId} via {Channel}",
                    notification.Id, channel);
            }

            await _repository.UpdateAsync(notification, cancellationToken);
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Determines the notification recipient based on event data and channel.
    /// </summary>
    private static string ResolveRecipient(EventMessage message, EventType eventType)
    {
        // For email-based events, use the email field.
        // For Slack/SMS, use userId as a fallback identifier.
        return !string.IsNullOrWhiteSpace(message.Email)
            ? message.Email
            : message.UserId;
    }
}
