using ShoppingListService.Services;
namespace CatalogService.RequestHelpers
{
#nullable enable
    public static class StructuredLogger
    {
        public static void LogInformation(
        ILogger logger, string message, IRequestContextService context, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Information, message, context, additionalData);
        }

        public static void LogWarning(
        ILogger logger, string message, IRequestContextService context, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Warning, message, context, additionalData);
        }

        public static void LogError(ILogger logger, string message, IRequestContextService context, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Error, message, context, additionalData);
        }

        public static void LogCritical(ILogger logger, string message, IRequestContextService context, Dictionary<string, object>? additionalData = null)
        {
            Log(logger, LogLevel.Critical, message, context, additionalData);
        }

        private static void Log(
        ILogger logger, LogLevel level, string message, IRequestContextService context, Dictionary<string, object>? additionalData)
        {
            var logData = new Dictionary<string, object>
        {
            { "traceId", context.TraceId },
            { "requestId", context.OperationId },
            { "method", context.RequestMethod },
            { "path", context.RequestPath },
            { "ownerId", context.UserId },
            { "family", context.FamilyName }
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
