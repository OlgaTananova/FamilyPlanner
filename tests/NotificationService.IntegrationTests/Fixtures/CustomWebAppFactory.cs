using System;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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
            // Add SignalR hub to the test services
            services.AddSignalR();

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

//             services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = false,
//         ValidateAudience = false,
//         ValidateIssuerSigningKey = true,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKey1234567891011444422876")) // Use your key
//     };
// });



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
          endpoints.MapHub<NotificationHub>("notifications");
      });
  });
    }

    async Task IAsyncLifetime.DisposeAsync() => await Task.CompletedTask;
}
