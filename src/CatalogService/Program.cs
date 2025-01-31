using System.Net;
using System.Text.Json;
using AutoMapper;
using CatalogService.Data;
using CatalogService.RequestHelpers;
using Contracts.Authentication;
using Contracts.Catalog;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<CatalogDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Telemetry
builder.Services.AddApplicationInsightsTelemetry();

// Configure built-in logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: (config) =>
{
    config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
}, options =>
{
    options.IncludeScopes = true;
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddExceptionHandler<GlobalErrorHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("ClientApps").Get<string[]>()) // Allow only these origins
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMassTransit(x =>
{
    // Set the endpoint name formatter to use kebab case
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("catalog", false));

    // Outbox for messages if the rabbitmq is not avaliable
    x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Handle<RabbitMqConnectionException>();
            r.Interval(5, TimeSpan.FromSeconds(10));
        });
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:User"]);
            h.Password(builder.Configuration["RabbitMq:Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Configure authentication with Azure AD B2C
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
    },
    options => builder.Configuration.Bind("AzureAdB2C", options));
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // IsAdmin Policy
    options.AddPolicy("IsAdmin", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "admin" && c.Value == "true"));
    });

    // IsAdult Policy
    options.AddPolicy("IsAdult", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "role" &&
                (c.Value == "Parent" || c.Value == "Other member")));
    });
}
);
// Enrich telemetry with data
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
// Enable CORS
app.UseExceptionHandler(o => { });
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine($"There is an error at db initialization. Message: {e.Message}");
}

try
{
    app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    if (DbInitializer.isNewDatabase)
    {
        await SendData.SendDataToShoppingListService(context, mapper, publishEndpoint);
    }

});
}
catch (Exception e)
{
    Console.WriteLine($"There is an error at sending data to ShoppingListService. Message: {e.Message}");
}


app.Run();


public partial class Program
{

}