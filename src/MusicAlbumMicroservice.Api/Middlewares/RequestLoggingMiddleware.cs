using System.Diagnostics;

namespace MusicAlbumMicroservice.Api.Middlewares;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = context.TraceIdentifier
        });
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;
            return Task.CompletedTask;
        });

        var started = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        finally
        {
            if (context.RequestAborted.IsCancellationRequested)
                _logger.LogInformation("HTTP {Method} {Path} was cancelled.", context.Request.Method, context.Request.Path);
            else
                _logger.LogInformation("HTTP {Method} {Path} returned {StatusCode} in {ElapsedMs:F1} ms",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode,
                    Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
    }
}
