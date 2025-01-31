using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
#nullable enable
namespace CatalogService.RequestHelpers
{
    public static class StructuredLogger
    {
        public static void LogInformation(
            ILogger logger, HttpContext httpContext, string message, string ownerId, string family, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Information, httpContext, message, ownerId, family, additionalData);
        }

        public static void LogWarning(
            ILogger logger, HttpContext httpContext, string message, string ownerId, string family, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Warning, httpContext, message, ownerId, family, additionalData);
        }

        public static void LogError(
            ILogger logger, HttpContext httpContext, string message, string ownerId, string family, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Error, httpContext, message, ownerId, family, additionalData);
        }

        public static void LogCritical(
            ILogger logger, HttpContext httpContext, string message, string ownerId, string family, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Critical, httpContext, message, ownerId, family, additionalData);
        }

        private static void Log(
            ILogger logger, LogLevel level, HttpContext httpContext, string message, string ownerId, string family, Dictionary<string, object>? additionalData)
        {
            string? traceId = httpContext.Features.Get<IHttpActivityFeature>()?.Activity.TraceId.ToString();
            if (string.IsNullOrEmpty(traceId))
            {
                traceId = httpContext.TraceIdentifier;
            }

            string requestId = httpContext.TraceIdentifier;

            var logData = new Dictionary<string, object>
            {
                { "traceId", traceId },
                { "requestId", requestId },
                { "method", httpContext.Request.Method },
                { "path", httpContext.Request.Path },
                { "ownerId", ownerId },
                { "family", family }
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    logData[kvp.Key] = kvp.Value;
                }
            }

            logger.Log(level, "{Message} | {@LogData}", message, logData);
        }
    }
}
