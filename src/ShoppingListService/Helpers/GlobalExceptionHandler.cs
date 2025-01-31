using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace ShoppingListService.Helpers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred while processing request.");

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var errorResponse = new
        {
            message = "An unexpected error occurred. Please try again later.",
            error = exception.Message,
            statusCode = httpContext.Response.StatusCode
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse);

        await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

        return true;
    }
}
