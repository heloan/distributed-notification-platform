using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Gateway.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Gateway.Application.Interfaces;
using Moq;

namespace Gateway.Api.Tests.Endpoints;

/// <summary>
/// Integration tests for the API Gateway endpoints.
/// Uses WebApplicationFactory to test the full HTTP pipeline.
/// </summary>
public class EventEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EventEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithMock(Action<Mock<IEventService>> setupMock)
    {
        var eventServiceMock = new Mock<IEventService>();
        setupMock(eventServiceMock);

        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real IEventService registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IEventService));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton(eventServiceMock.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostEvent_ValidRequest_Returns202Accepted()
    {
        // Arrange
        var expectedResponse = new EventResponse(
            "abc-123", "USER_REGISTERED", "123", "user@email.com", DateTime.UtcNow, "CREATED");

        var client = CreateClientWithMock(mock =>
            mock.Setup(s => s.SendEventAsync(It.IsAny<EventRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse));

        var request = new EventRequest("USER_REGISTERED", "123", "user@email.com", DateTime.UtcNow);

        // Act
        var response = await client.PostAsJsonAsync("/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var body = await response.Content.ReadFromJsonAsync<EventResponse>();
        body.Should().NotBeNull();
        body!.EventType.Should().Be("USER_REGISTERED");
    }

    [Fact]
    public async Task PostEvent_InvalidEventType_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClientWithMock(_ => { });
        var request = new EventRequest("INVALID_TYPE", "123", "user@email.com");

        // Act
        var response = await client.PostAsJsonAsync("/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostEvent_MissingEmail_Returns400BadRequest()
    {
        // Arrange
        var client = CreateClientWithMock(_ => { });
        var request = new EventRequest("USER_REGISTERED", "123", "");

        // Act
        var response = await client.PostAsJsonAsync("/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEvents_ReturnsOkWithEvents()
    {
        // Arrange
        var events = new[]
        {
            new EventResponse("1", "USER_REGISTERED", "123", "a@b.com", DateTime.UtcNow, "CREATED"),
        };

        var client = CreateClientWithMock(mock =>
            mock.Setup(s => s.GetEventsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(events));

        // Act
        var response = await client.GetAsync("/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHealth_ReturnsOkHealthy()
    {
        // Arrange
        var client = CreateClientWithMock(_ => { });

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
