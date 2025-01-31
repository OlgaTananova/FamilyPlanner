using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Helpers;
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception in Notification Service.");

        // If an HTTP context exists (e.g., SignalR)
        if (httpContext != null)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var errorResponse = new
            {
                message = "An unexpected error occurred in Notification Service.",
                error = exception.Message,
                statusCode = httpContext.Response.StatusCode
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);
        }

        return true;
    }
}

