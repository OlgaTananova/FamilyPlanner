using System;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace CatalogService.RequestHelpers;

public class GlobalErrorHandler : IExceptionHandler
{
    private readonly ILogger<GlobalErrorHandler> _logger;
    private ProblemDetailsFactory _problemDetailsFactory;

    public GlobalErrorHandler(
        ILogger<GlobalErrorHandler> logger, ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = StatusCodes.Status500InternalServerError;
        string title = "An unexpected error occurred";
        string detail = exception.Message;

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
        httpContext,
        statusCode: statusCode,
        title: title,
        detail: detail,
        instance: httpContext.Request.Path
    );

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
