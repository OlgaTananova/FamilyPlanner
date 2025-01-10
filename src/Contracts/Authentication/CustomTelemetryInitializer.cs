using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Contracts.Authentication;

public class CustomTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            telemetry.Context.GlobalProperties["UserId"] = httpContext.User?.FindFirst("userId")?.Value;
            telemetry.Context.GlobalProperties["FamilyName"] = httpContext.User?.FindFirst("family")?.Value;
        }
    }
}
