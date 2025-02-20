using CatalogService.Data;
using CatalogService.IntegrationTests.Utils;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace CatalogService.IntegrationTests.Fixtures;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _posgresSqlContainer = new PostgreSqlBuilder()
     .WithDatabase("test_catalog_db").Build();
    public async Task InitializeAsync()
    {
        await _posgresSqlContainer.StartAsync();
    }

    // Create testing version of the application from Program class
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {

            services.RemoveTelemetry();
            services.RemoveDbContext<CatalogDbContext>();
            services.AddDbContext<CatalogDbContext>(options =>
            {
                var connectionString = _posgresSqlContainer.GetConnectionString();

                options.UseNpgsql(connectionString);
            });
            // Replace the message broker
            services.AddMassTransitTestHarness();

            // Add Migration to the testing database
            services.EnsureCreated<CatalogDbContext>();

            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });


        });
    }

    Task IAsyncLifetime.DisposeAsync() => _posgresSqlContainer.DisposeAsync().AsTask();

}
