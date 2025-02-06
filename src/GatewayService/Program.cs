using Microsoft.Identity.Web;
using Yarp.ReverseProxy;
using AspNetCoreRateLimit;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Load reverse proxy configuration based on environment
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Logging and Telemetry
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) =>
        { config.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]; },
    configureApplicationInsightsLoggerOptions: (options) =>
    {
        options.IncludeScopes = true;
    });

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
    },
    options => builder.Configuration.Bind("AzureAdB2C", options));

// Add authorization
builder.Services.AddAuthorization();

// Add configuration for rate limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", b =>
    {
        b.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins(builder.Configuration.GetSection("ClientApps").Get<string[]>()!);
    });
});

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("customPolicy");
app.UseIpRateLimiting(); // Add Rate Limiting Middleware
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();
