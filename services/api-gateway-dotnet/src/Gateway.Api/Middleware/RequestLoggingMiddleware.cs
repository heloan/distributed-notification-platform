using System.Diagnostics;

namespace Gateway.Api.Middleware;

/// <summary>
/// Middleware that logs request/response information and measures request duration.
/// Provides structured logging for observability.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogInformation(
            "→ {Method} {Path} | RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            requestId);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "← {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | RequestId: {RequestId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            requestId);
    }
}
