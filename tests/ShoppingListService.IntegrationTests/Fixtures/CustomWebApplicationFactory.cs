using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingListService.Consumers;
using ShoppingListService.Data;
using ShoppingListService.Helpers;
using ShoppingListService.IntegrationTests.Utils;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace ShoppingListService.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _posgresSqlContainer = new PostgreSqlBuilder()
     .WithDatabase("test_shoppingList_db").Build();
    public async Task InitializeAsync()
    {
        await _posgresSqlContainer.StartAsync();
    }

    // Create testing version of the application from Program class
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.RemoveDbContext<ShoppingListContext>();
            // Disable Application Insights telemetry
            services.RemoveTelemetry();
            services.AddDbContext<ShoppingListContext>(options =>
            {
                var connectionString = _posgresSqlContainer.GetConnectionString();

                options.UseNpgsql(connectionString);
            });

            services.AddMassTransitTestHarness(options =>
            {
                options.AddConsumer<CatalogItemCreatedConsumer>();
                options.AddConsumer<CatalogCategoryUpdatedConsumer>();
            });

            // Add Migration to the testing database
            services.EnsureCreated<ShoppingListContext>();

            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });

        });
    }

    Task IAsyncLifetime.DisposeAsync() => _posgresSqlContainer.DisposeAsync().AsTask();

}
