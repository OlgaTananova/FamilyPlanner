using System;
using System.Security.Claims;
using CatalogService.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        DbHelper.InitDbForTests(db);
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
