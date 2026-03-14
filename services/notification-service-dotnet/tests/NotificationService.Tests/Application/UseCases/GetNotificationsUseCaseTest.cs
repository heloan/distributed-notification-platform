using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Repositories;

namespace NotificationService.Tests.Application.UseCases;

/// <summary>
/// Unit tests for <see cref="GetNotificationsUseCase"/>.
/// </summary>
public sealed class GetNotificationsUseCaseTest
{
    private readonly Mock<INotificationRepository> _repository = new();
    private readonly Mock<ILogger<GetNotificationsUseCase>> _logger = new();
    private readonly GetNotificationsUseCase _sut;

    public GetNotificationsUseCaseTest()
    {
        _sut = new GetNotificationsUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnMappedResponses()
    {
        // Arrange
        var n1 = Notification.Create(Guid.NewGuid(), ChannelType.Email, "a@b.com", "Hello");
        var n2 = Notification.Create(Guid.NewGuid(), ChannelType.Slack, "#general", "Alert");

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { n1, n2 });

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Channel.Should().Be("EMAIL");
        result[1].Channel.Should().Be("SLACK");
    }

    [Fact]
    public async Task GetByEventId_ShouldReturnFiltered()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var n1 = Notification.Create(eventId, ChannelType.Email, "a@b.com", "msg");

        _repository.Setup(r => r.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { n1 });

        // Act
        var result = await _sut.GetByEventIdAsync(eventId);

        // Assert
        result.Should().ContainSingle();
        result[0].EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoNotifications()
    {
        // Arrange
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Notification>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
