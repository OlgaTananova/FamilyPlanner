using System;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Consumers;
using NotificationService.Hubs;
using NotificationService.IntegrationTests.Utils;
using WebMotions.Fake.Authentication.JwtBearer;

namespace NotificationService.IntegrationTests.Fixtures;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    public async Task InitializeAsync() => await Task.CompletedTask;

    // Create testing version of the application from Program class
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {

            // Remove existing authentication to avoid duplicate registration
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IAuthenticationHandlerProvider>();

            // Add test authentication using a custom JWT bearer scheme
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey1234567891011444422876")), // Replace with a secure key
                        ValidateLifetime = false
                    };
                });
            services.AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<CatalogCategoryUpdatedConsumer>();
                x.AddConsumer<CatalogCategoryCreatedConsumer>();
                x.AddConsumer<CatalogCategoryDeletedConsumer>();

                x.AddConsumer<CatalogItemCreatedConsumer>();
                x.AddConsumer<CatalogItemUpdatedConsumer>();
                x.AddConsumer<CatalogItemDeletedConsumer>();

                x.AddConsumer<ShoppingListCreatedConsumer>();
                x.AddConsumer<ShoppingListDeletedConsumer>();
                x.AddConsumer<ShoppingListUpdatedConsumer>();

                x.AddConsumer<ShoppingListItemUpdatedConsumer>();
                x.AddConsumer<ShoppingListItemsAddedConsumer>();
                x.AddConsumer<ShoppingListItemDeletedConsumer>();
            });

            // Add SignalR hub to the test services
            services.AddSignalR();

            // Disable Application Insights telemetry
            services.RemoveTelemetry();

        });
        builder.Configure(app =>
  {
      app.UseRouting();
      app.UseAuthentication();
      app.UseAuthorization();

      // Map the NotificationHub
      app.UseEndpoints(endpoints =>
      {
          // TODO add authentication
          endpoints.MapHub<NotificationHub>("notifications").RequireAuthorization();
      });
  });
    }

    async Task IAsyncLifetime.DisposeAsync() => await Task.CompletedTask;
}
