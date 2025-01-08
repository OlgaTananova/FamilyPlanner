using Microsoft.Identity.Web;
using Yarp.ReverseProxy;
using AspNetCoreRateLimit;
using Microsoft.ApplicationInsights.Extensibility;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
var appInsightsConnectionString = Environment.GetEnvironmentVariable("APP_INSIGHTS_CONNECTION_STRING");

builder.Services.AddApplicationInsightsTelemetry();

// Configure logging to use Application Insights
builder.Logging.AddApplicationInsights(
    configureTelemetryConfiguration: (config) =>
        config.ConnectionString = appInsightsConnectionString,
    configureApplicationInsightsLoggerOptions: (options) =>
    {
        options.IncludeScopes = true; // Enable scopes for structured logging
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


builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", b =>
    {
        b.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:3000");
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
