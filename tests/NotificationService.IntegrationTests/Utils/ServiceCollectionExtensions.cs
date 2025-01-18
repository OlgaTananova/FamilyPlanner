using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NotificationService.IntegrationTests.Utils;

public static class ServiceCollectionExtensions
{
    public static void RemoveTelemetry(this IServiceCollection services)
    {
        // Remove Application Insights services
        var telemetryDescriptors = services.Where(
            s => s.ServiceType.FullName?.StartsWith("Microsoft.ApplicationInsights") == true).ToList();

        foreach (var descriptor in telemetryDescriptors)
        {
            services.Remove(descriptor);
        }

        // Remove TelemetryChannel to stop data transmission
        services.RemoveAll<ITelemetryChannel>();
        services.RemoveAll(typeof(ITelemetryInitializer));

    }
}
