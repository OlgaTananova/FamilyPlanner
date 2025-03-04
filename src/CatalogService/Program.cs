using System.Diagnostics;
using AutoMapper;
using CatalogService.Data;
using CatalogService.RequestHelpers;
using CatalogService.Services;
using Contracts.Authentication;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
if (builder.Environment.IsProduction())
{

    var envFilePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env.prod"));

    if (File.Exists(envFilePath))
    {
        DotNetEnv.Env.Load(envFilePath);
        Console.WriteLine(".env.prod file loaded successfully!");
    }
    else
    {
        Console.WriteLine($".env.prod file not found at: {envFilePath}");
    }
}

// Load environment-specific configuration into AppConfig
var appConfig = AppConfig.LoadConfiguration(builder.Configuration, builder.Environment);

// Register AppConfig as a singleton
builder.Services.AddSingleton(appConfig);
builder.Services.AddControllers();

builder.Services.AddDbContext<CatalogDbContext>(opt =>
{
    opt.UseNpgsql(appConfig.DefaultConnectionString);
});

// Add Telemetry
builder.Services.AddApplicationInsightsTelemetry();

// Configure built-in logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights((configureTelemetryConfiguration) =>
{
    Console.WriteLine($"App Insights Connection String: {appConfig.ApplicationInsightsConnectionString ?? "Not Found"}");
    configureTelemetryConfiguration.ConnectionString = appConfig.ApplicationInsightsConnectionString;
}, options =>
{
    options.IncludeScopes = true;
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddExceptionHandler<GlobalErrorHandler>();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

#nullable enable

        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(appConfig.ClientApps) // Allow only these origins
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

        cfg.Host(appConfig.RabbitMqHost, "/", h =>
        {
            h.Username(appConfig.RabbitMqUser);
            h.Password(appConfig.RabbitMqPassword);
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Configure authentication with Azure AD B2C
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => { }, options =>
   {
       AppConfig.LoadAzureAdB2CConfig(options, appConfig, builder.Configuration, builder.Environment);
   });
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
        context.User.HasClaim(c => c.Type == "role" && (c.Value == "Parent" || c.Value == "Other member")));
    });
}
);
// Allow httpcontext to be available in the services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
builder.Services.AddScoped<IRequestContextService, RequestContextService>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<ICatalogBusinessService, CatalogBusinessService>();
builder.Services.AddScoped<IGraphServiceClientFactory, GraphServiceClientFactory>();
builder.Services.AddScoped<IGraphService, GraphService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
// Enable CORS
app.UseExceptionHandler(o => { });
app.UseStatusCodePages();
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