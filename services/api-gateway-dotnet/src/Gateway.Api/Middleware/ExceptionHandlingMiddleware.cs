using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Gateway.Application.DTOs;

namespace Gateway.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches unhandled exceptions and returns consistent error responses.
/// Follows Single Responsibility — only handles exception-to-response mapping.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Downstream service communication error");

            var statusCode = ex.StatusCode switch
            {
                System.Net.HttpStatusCode.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
                System.Net.HttpStatusCode.GatewayTimeout => StatusCodes.Status504GatewayTimeout,
                _ => StatusCodes.Status502BadGateway
            };

            await WriteErrorResponse(context, new ErrorResponse(
                Title: statusCode == StatusCodes.Status503ServiceUnavailable
                    ? "Service Unavailable"
                    : "Bad Gateway",
                Status: statusCode,
                Detail: $"The downstream service returned an error: {ex.Message}"));
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Downstream service request timed out");

            await WriteErrorResponse(context, new ErrorResponse(
                Title: "Gateway Timeout",
                Status: StatusCodes.Status504GatewayTimeout,
                Detail: "The downstream service did not respond in time."));
        }
        catch (OperationCanceledException)
        {
            // Client cancelled the request — no need to log as error
            _logger.LogInformation("Request was cancelled by the client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");

            await WriteErrorResponse(context, new ErrorResponse(
                Title: "Internal Server Error",
                Status: StatusCodes.Status500InternalServerError,
                Detail: "An unexpected error occurred. Please try again later."));
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, ErrorResponse error)
    {
        context.Response.StatusCode = error.Status;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        await context.Response.WriteAsJsonAsync(error, options);
    }
}
