using System;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CatalogService.RequestHelpers;

public class GlobalErrorHandler : IExceptionHandler
{
    private readonly ILogger<GlobalErrorHandler> _logger;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public GlobalErrorHandler(
        ILogger<GlobalErrorHandler> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }
  
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred while processing request.");

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred",
            detail: exception.Message
        );

        httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
