using FluentAssertions;
using NotificationService.Application.Mappers;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Tests.Application.Mappers;

/// <summary>
/// Unit tests for <see cref="NotificationMapper"/>.
/// </summary>
public sealed class NotificationMapperTest
{
    [Fact]
    public void ToResponse_ShouldMapAllFields()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Email, "user@test.com", "Welcome!");

        // Act
        var response = NotificationMapper.ToResponse(notification);

        // Assert
        response.Id.Should().Be(notification.Id);
        response.EventId.Should().Be(notification.EventId);
        response.Channel.Should().Be("EMAIL");
        response.Recipient.Should().Be("user@test.com");
        response.Status.Should().Be("PENDING");
        response.Message.Should().Be("Welcome!");
        response.SentAt.Should().BeNull();
        response.CreatedAt.Should().Be(notification.CreatedAt);
    }

    [Fact]
    public void ToResponse_ShouldMapSentStatus()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(), ChannelType.Slack, "#alerts", "Alert!");
        notification.MarkAsSent();

        // Act
        var response = NotificationMapper.ToResponse(notification);

        // Assert
        response.Status.Should().Be("SENT");
        response.SentAt.Should().NotBeNull();
    }
}
