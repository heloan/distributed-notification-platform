using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;

namespace NotificationService.Tests.Application.UseCases;

/// <summary>
/// Unit tests for <see cref="ProcessEventUseCase"/>.
/// Verifies the orchestration logic without touching infrastructure.
/// </summary>
public sealed class ProcessEventUseCaseTest
{
    private readonly Mock<IRuleEngine> _ruleEngine = new();
    private readonly Mock<INotificationSender> _emailSender = new();
    private readonly Mock<INotificationSender> _slackSender = new();
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<ILogger<ProcessEventUseCase>> _logger = new();
    private readonly ProcessEventUseCase _sut;

    public ProcessEventUseCaseTest()
    {
        _emailSender.Setup(s => s.Channel).Returns(ChannelType.Email);
        _slackSender.Setup(s => s.Channel).Returns(ChannelType.Slack);

        _sut = new ProcessEventUseCase(
            _ruleEngine.Object,
            new[] { _emailSender.Object, _slackSender.Object },
            _repository.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Execute_ShouldSendEmail_WhenRuleMatchesEmailChannel()
    {
        // Arrange
        var message = new EventMessage(
            Guid.NewGuid().ToString(), "USER_REGISTERED", "user-1", "u@e.com", DateTime.UtcNow);

        var rule = new NotificationRule(
            EventType.UserRegistered,
            new[] { ChannelType.Email },
            "Welcome!");

        _ruleEngine.Setup(r => r.Evaluate(EventType.UserRegistered)).Returns(rule);
        _emailSender.Setup(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.ExecuteAsync(message);

        // Assert
        _repository.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Sent),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldSendToMultipleChannels_WhenRuleHasMultipleChannels()
    {
        // Arrange
        var message = new EventMessage(
            Guid.NewGuid().ToString(), "SECURITY_ALERT", "user-1", "u@e.com", DateTime.UtcNow);

        var rule = new NotificationRule(
            EventType.SecurityAlert,
            new[] { ChannelType.Email, ChannelType.Slack },
            "Security alert");

        _ruleEngine.Setup(r => r.Evaluate(EventType.SecurityAlert)).Returns(rule);
        _emailSender.Setup(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _slackSender.Setup(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.ExecuteAsync(message);

        // Assert
        _repository.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _emailSender.Verify(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
        _slackSender.Verify(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldSkip_WhenEventTypeIsUnknown()
    {
        // Arrange
        var message = new EventMessage(
            Guid.NewGuid().ToString(), "UNKNOWN_TYPE", "user-1", "u@e.com", DateTime.UtcNow);

        // Act
        await _sut.ExecuteAsync(message);

        // Assert
        _ruleEngine.Verify(r => r.Evaluate(It.IsAny<EventType>()), Times.Never);
        _repository.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldSkip_WhenNoRuleExists()
    {
        // Arrange
        var message = new EventMessage(
            Guid.NewGuid().ToString(), "USER_REGISTERED", "user-1", "u@e.com", DateTime.UtcNow);

        _ruleEngine.Setup(r => r.Evaluate(EventType.UserRegistered)).Returns((NotificationRule?)null);

        // Act
        await _sut.ExecuteAsync(message);

        // Assert
        _repository.Verify(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldMarkAsFailed_WhenSenderThrowsException()
    {
        // Arrange
        var message = new EventMessage(
            Guid.NewGuid().ToString(), "USER_REGISTERED", "user-1", "u@e.com", DateTime.UtcNow);

        var rule = new NotificationRule(
            EventType.UserRegistered,
            new[] { ChannelType.Email },
            "Welcome!");

        _ruleEngine.Setup(r => r.Evaluate(EventType.UserRegistered)).Returns(rule);
        _emailSender.Setup(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP down"));

        // Act
        await _sut.ExecuteAsync(message);

        // Assert
        _repository.Verify(r => r.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Failed),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldMarkAsFailed_WhenSenderReturnsFalse()
    {
        // Arrange
        var message = new EventMessage(
            Guid.NewGuid().ToString(), "USER_REGISTERED", "user-1", "u@e.com", DateTime.UtcNow);

        var rule = new NotificationRule(
            EventType.UserRegistered,
            new[] { ChannelType.Email },
            "Welcome!");

        _ruleEngine.Setup(r => r.Evaluate(EventType.UserRegistered)).Returns(rule);
        _emailSender.Setup(s => s.SendAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.ExecuteAsync(message);

        // Assert
        _repository.Verify(r => r.UpdateAsync(
            It.Is<Notification>(n => n.Status == NotificationStatus.Failed),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
