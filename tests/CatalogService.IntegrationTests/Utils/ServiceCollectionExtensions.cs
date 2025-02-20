using CatalogService.Data;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CatalogService.IntegrationTests.Utils;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<T>(this IServiceCollection services)
    {
        // Remove real db context
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CatalogDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    public static void EnsureCreated<T>(this IServiceCollection services)
    {
        // Add Migration to the testing database
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<CatalogDbContext>();
        db.Database.Migrate();
        // Ensure the database is seeded only once
        if (!db.Categories.Any())
        {
            DbHelper.InitDbForTests(db);
        }
    }
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

        // Clear Logging Providers

        services.AddLogging(loggingbuilder =>
        {
            loggingbuilder.ClearProviders();
            loggingbuilder.AddConsole();
        });

    }

    public static void RemoveClaimTransformation(this IServiceCollection services)
    {
        // Remove the real IClaimsTransformation and replace it with a mocked one if needed
        var descriptors = services.Where(
        d => d.ServiceType == typeof(IClaimsTransformation)).ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
