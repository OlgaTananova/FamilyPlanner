using Microsoft.AspNetCore.Http.Features;

namespace ShoppingListService.Services;

public class RequestContextService : IRequestContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext => _httpContextAccessor.HttpContext;

    public string UserId => HttpContext?.User?.FindFirst("userId")?.Value ?? "Unknown";
    public string FamilyName => HttpContext?.User?.FindFirst("family")?.Value ?? "Unknown";
    public string OperationId => HttpContext?.Features.Get<IHttpActivityFeature>()?.Activity.TraceId.ToString() ?? "Unknown";
    public string TraceId => HttpContext?.Features.Get<IHttpActivityFeature>()?.Activity.TraceId.ToString() ?? "Unknown";

    public string RequestId => HttpContext?.TraceIdentifier ?? "Unknown";
    public string RequestMethod => HttpContext?.Request.Method ?? "Unknown";
    public string RequestPath => HttpContext?.Request.Path ?? "Unknown";
}
