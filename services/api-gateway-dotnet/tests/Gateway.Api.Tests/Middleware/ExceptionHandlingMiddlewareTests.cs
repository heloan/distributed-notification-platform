using System.Net;
using System.Text.Json;
using FluentAssertions;
using Gateway.Api.Middleware;
using Gateway.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gateway.Api.Tests.Middleware;

/// <summary>
/// Unit tests for ExceptionHandlingMiddleware.
/// Verifies correct error response mapping for different exception types.
/// </summary>
public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock = new();

    [Fact]
    public async Task InvokeAsync_NoException_PassesThrough()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            next: _ => Task.CompletedTask,
            logger: _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_HttpRequestException_Returns502()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            next: _ => throw new HttpRequestException("Connection refused"),
            logger: _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(502);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            next: _ => throw new InvalidOperationException("Something broke"),
            logger: _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_OperationCancelled_DoesNotWriteResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var middleware = new ExceptionHandlingMiddleware(
            next: _ => throw new OperationCanceledException(),
            logger: _loggerMock.Object);

        // Act & Assert — should not throw
        await middleware.InvokeAsync(context);
    }
}
