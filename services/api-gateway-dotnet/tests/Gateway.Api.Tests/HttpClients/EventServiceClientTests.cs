using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Gateway.Application.DTOs;
using Gateway.Application.Interfaces;
using Gateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Gateway.Api.Tests.HttpClients;

/// <summary>
/// Unit tests for EventServiceClient.
/// Uses mocked HttpClient to test HTTP communication logic.
/// </summary>
public class EventServiceClientTests
{
    private readonly Mock<ILogger<EventServiceClient>> _loggerMock = new();

    private EventServiceClient CreateClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        return new EventServiceClient(httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task SendEventAsync_SuccessfulResponse_ReturnsEventResponse()
    {
        // Arrange
        var expectedResponse = new EventResponse(
            Id: "abc-123",
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "user@email.com",
            Timestamp: DateTime.UtcNow,
            Status: "CREATED");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(expectedResponse)
        };

        var client = CreateClient(httpResponse);

        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "user@email.com");

        // Act
        var result = await client.SendEventAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("abc-123");
        result.EventType.Should().Be("USER_REGISTERED");
    }

    [Fact]
    public async Task SendEventAsync_ServerError_ThrowsHttpRequestException()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var client = CreateClient(httpResponse);

        var request = new EventRequest(
            EventType: "USER_REGISTERED",
            UserId: "123",
            Email: "user@email.com");

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.SendEventAsync(request));
    }

    [Fact]
    public async Task GetEventsAsync_SuccessfulResponse_ReturnsEvents()
    {
        // Arrange
        var events = new[]
        {
            new EventResponse("1", "USER_REGISTERED", "123", "a@b.com", DateTime.UtcNow, "CREATED"),
            new EventResponse("2", "ORDER_SHIPPED", "456", "c@d.com", DateTime.UtcNow, "CREATED")
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(events)
        };

        var client = CreateClient(httpResponse);

        // Act
        var result = await client.GetEventsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventByIdAsync_Found_ReturnsEvent()
    {
        // Arrange
        var expectedEvent = new EventResponse(
            "abc-123", "USER_REGISTERED", "123", "user@email.com", DateTime.UtcNow, "CREATED");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(expectedEvent)
        };

        var client = CreateClient(httpResponse);

        // Act
        var result = await client.GetEventByIdAsync("abc-123");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("abc-123");
    }

    [Fact]
    public async Task GetEventByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
        var client = CreateClient(httpResponse);

        // Act
        var result = await client.GetEventByIdAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }
}
