using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.RequestHelpers
{
    public static class ProblemDetailsFactoryHelper
    {
        public static ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int statusCode,
            string title,
            string detail)
        {

            string traceId = httpContext.Request.Headers["traceparent"].ToString();
            if (string.IsNullOrEmpty(traceId))
            {
                traceId = httpContext.TraceIdentifier;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;

            return problemDetails;
        }
    }
}
