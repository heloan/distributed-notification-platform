using FluentAssertions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="NotificationRule"/> entity.
/// </summary>
public sealed class NotificationRuleTest
{
    [Fact]
    public void Constructor_ShouldCreateRule_WithValidParameters()
    {
        // Arrange & Act
        var rule = new NotificationRule(
            EventType.UserRegistered,
            new[] { ChannelType.Email },
            "Welcome!");

        // Assert
        rule.EventType.Should().Be(EventType.UserRegistered);
        rule.Channels.Should().ContainSingle().Which.Should().Be(ChannelType.Email);
        rule.MessageTemplate.Should().Be("Welcome!");
    }

    [Fact]
    public void Constructor_ShouldAcceptMultipleChannels()
    {
        // Arrange & Act
        var rule = new NotificationRule(
            EventType.SecurityAlert,
            new[] { ChannelType.Email, ChannelType.Slack },
            "Security alert");

        // Assert
        rule.Channels.Should().HaveCount(2);
        rule.Channels.Should().Contain(ChannelType.Email);
        rule.Channels.Should().Contain(ChannelType.Slack);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMessageTemplateIsEmpty()
    {
        // Act
        var act = () => new NotificationRule(
            EventType.OrderShipped,
            new[] { ChannelType.Sms },
            "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("messageTemplate");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenChannelsAreEmpty()
    {
        // Act
        var act = () => new NotificationRule(
            EventType.OrderShipped,
            Array.Empty<ChannelType>(),
            "Order shipped");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("channels");
    }

    [Fact]
    public void ToString_ShouldContainEventTypeAndChannels()
    {
        // Arrange
        var rule = new NotificationRule(
            EventType.PaymentFailed,
            new[] { ChannelType.Slack },
            "Payment failed");

        // Act
        var result = rule.ToString();

        // Assert
        result.Should().Contain("PaymentFailed");
        result.Should().Contain("Slack");
    }
}
