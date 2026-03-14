using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Providers;

/// <summary>
/// Simulated email notification provider.
/// In production, this would integrate with SendGrid, AWS SES, etc.
/// Follows Liskov Substitution — interchangeable with other <see cref="INotificationSender"/> implementations.
/// </summary>
public sealed class EmailProvider : INotificationSender
{
    private readonly ILogger<EmailProvider> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="EmailProvider"/>.
    /// </summary>
    public EmailProvider(ILogger<EmailProvider> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ChannelType Channel => ChannelType.Email;

    /// <inheritdoc />
    public Task<bool> SendAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📧 [EMAIL] Sending to {Recipient}: {Message}",
            notification.Recipient,
            notification.Message);

        // Simulate successful delivery
        return Task.FromResult(true);
    }
}
