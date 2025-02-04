using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CatalogService.RequestHelpers
{
    public static class ProblemDetailsFactoryHelper
    {
        public static ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int statusCode,
            string title,
            string detail,
            ProblemDetailsFactory problemDetailsFactory)
        {

            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: statusCode,
                title: title,
                detail: detail,
                instance: $"{httpContext.Request.Method} {httpContext.Request.Path}"
            );

            return problemDetails;
        }
    }
}