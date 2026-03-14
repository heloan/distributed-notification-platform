using FluentAssertions;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="Notification"/> domain entity.
/// </summary>
public sealed class NotificationTest
{
    // -----------------------------------------------------------------------
    // Create
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnNotification_WithPendingStatus()
    {
        // Arrange & Act
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Email, "user@test.com", "Welcome!");

        // Assert
        notification.Id.Should().NotBeEmpty();
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.SentAt.Should().BeNull();
        notification.Channel.Should().Be(ChannelType.Email);
        notification.Recipient.Should().Be("user@test.com");
        notification.Message.Should().Be("Welcome!");
    }

    [Fact]
    public void Create_ShouldThrow_WhenRecipientIsEmpty()
    {
        // Act
        var act = () => Notification.Create(
            Guid.NewGuid(), ChannelType.Email, "", "message");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("recipient");
    }

    [Fact]
    public void Create_ShouldThrow_WhenMessageIsEmpty()
    {
        // Act
        var act = () => Notification.Create(
            Guid.NewGuid(), ChannelType.Slack, "user@test.com", "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("message");
    }

    // -----------------------------------------------------------------------
    // State Transitions
    // -----------------------------------------------------------------------

    [Fact]
    public void MarkAsSent_ShouldSetStatusAndSentAt()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Email, "user@test.com", "Hello");

        // Act
        notification.MarkAsSent();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAt.Should().NotBeNull();
        notification.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatus()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Sms, "+1234567890", "Alert");

        // Act
        notification.MarkAsFailed();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.SentAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsRetrying_ShouldSetStatus()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Slack, "#channel", "Alert");

        // Act
        notification.MarkAsRetrying();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Retrying);
    }

    // -----------------------------------------------------------------------
    // Identity Equality
    // -----------------------------------------------------------------------

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSameId()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Email, "a@b.com", "msg");

        // Act & Assert
        notification.Equals(notification).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldContainKeyFields()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Email, "a@b.com", "msg");

        // Act
        var result = notification.ToString();

        // Assert
        result.Should().Contain("Notification");
        result.Should().Contain("Email");
        result.Should().Contain("Pending");
    }
}
