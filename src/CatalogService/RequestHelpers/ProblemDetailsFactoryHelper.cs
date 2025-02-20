using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CatalogService.RequestHelpers
{
    public static class ProblemDetailsFactoryHelper
    {
        public static ProblemDetails CreateProblemDetails(
            HttpContext context,
            int statusCode,
            string title,
            string detail,
            ProblemDetailsFactory problemDetailsFactory)
        {

            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext: context,
                statusCode: statusCode,
                title: title,
                detail: detail,
                instance: $"{context.Request.Method} {context.Request.Path}"
            );

            return problemDetails;
        }
    }
}
